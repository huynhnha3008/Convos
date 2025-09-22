using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using BusinessObjects.QueryObject;
using BusinessObjects.DTOs.RoleDto;
using Microsoft.AspNetCore.Http.HttpResults;
using Services.SignalR.Interfaces;

namespace Services.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IRoleHubs _roleHubs;
        private readonly IUserService _userService;
        private readonly IServerMemberService _serverMemberService;
        private readonly IPermissionService _permissionService;

        public RoleController(IRoleService roleService, IUserService userService, IServerMemberService serverMemberService, IPermissionService permissionService)
        {
            _roleService = roleService;
            _userService = userService;
            _serverMemberService = serverMemberService;
            _permissionService = permissionService;
        }


        // GET: api/role/{id}
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleCreateResponse>> GetRoleById(Guid id)
        {
            RoleCreateResponse role = await _roleService.GetByIdAsync(id);
            if (role == null)
            {
                return NotFound("Role not found");
            }
            return Ok(role);
        }


        // GET: api/roles/members/{id}
        //[Authorize]
        [HttpGet("members/{id}")]
        public async Task<ActionResult<List<RoleCreateResponse>>> GetRolesByMemberId(Guid id)
        {
            try
            {
                List<RoleCreateResponse> roles = await _roleService.GetAllByMemberIdAsync(id);

                return Ok(roles);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }

        }

        // POST: api/role
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<RoleCreateResponse>> CreateRole([FromBody] RoleCreateRequest roleRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var createdRole = await _roleService.CreateAsync(roleRequest, currentUserId);
                return CreatedAtAction(nameof(GetRoleById), new { id = createdRole.Id }, createdRole);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }

        [Authorize]
        [HttpPut("change-role-position/server/{id}")]
        public async Task<IActionResult> ChangeServerRolePosition(Guid id, [FromBody] RolePositionUpdateRequest rolePositionUpdate)
        {

            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);

                var result = await _roleService.ChangeRolePositionInServerAsync(rolePositionUpdate.RoleId, rolePositionUpdate.NewPosition, id, currentUserId);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateRole(Guid id, [FromBody] RoleUpdateRequest roleUpdateRequest)
        {
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                await _roleService.UpdateAsync(id, currentUserId, roleUpdateRequest);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }


        // DELETE: api/role/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<RoleCreateResponse>> DeleteRole(Guid id)
        {

            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);

                await _roleService.DeleteAsync(id, currentUserId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }

        [Authorize]
        [HttpPost("{id}/assign/channels/{channel-id}")]
        public async Task<IActionResult> AssignRoleToChannel(Guid id, [FromRoute(Name = "channel-id")] Guid channelId)
        {
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                await _roleService.AddRoleToChannel(channelId, id, currentUserId);
                return Ok("ok");
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }

        }


        [Authorize]
        [HttpPost("{id}/unassign/channels/{channel-id}")]
        public async Task<IActionResult> UnAssignRoleToChannel(Guid id, [FromRoute(Name = "channel-id")] Guid channelId)
        {
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                await _roleService.RemoveRoleFromChannel(channelId, id, currentUserId);
                return Ok("ok");
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }

        }


        [Authorize]
        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole(Guid memberId, Guid roleId)
        {
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var createdMemberRole = await _roleService.AddMemberRole(memberId, roleId, currentUserId);
                return Ok(createdMemberRole);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }

        }


        [Authorize]
        [HttpPost("unassign")]
        public async Task<IActionResult> UnassignRole(Guid memberId, Guid roleId)
        {
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var removedMemberRole = await _roleService.RemoveMemberRole(memberId, roleId, currentUserId);
                return Ok(removedMemberRole);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }

        [Authorize]
        [HttpGet("server/{id}")]
        public async Task<IActionResult> GetAllByServerId(Guid id, [FromQuery] QueryRole query)
        {
            try
            {
                var roles = await _roleService.GetAllByServerIdAsync(id, query);
                return Ok(roles);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }


        [Authorize]
        [HttpPut("update-server-role-permission/{id}")]
        public async Task<IActionResult> UpdateRolePermissions(Guid id, [FromBody] List<UpdateRolePermissionDTO> updateRoleDTOs)
        {
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var roles = await _roleService.UpdatePermissionsToRoleAsync(id, updateRoleDTOs, currentUserId);

                return Ok(roles);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }

        [Authorize]
        [HttpPut("update-server-channel-role-permission")]
        public async Task<IActionResult> UpdateChannelRolePermissions([FromBody] UpdateChannelRolePermissionRequest updateChannelRolePermissionRequest)
        {
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var roles = await _roleService.UpdatePermissionsToChannelRoleAsync(updateChannelRolePermissionRequest.roleId, updateChannelRolePermissionRequest.rolePermissions, updateChannelRolePermissionRequest.channelId, currentUserId);
                return Ok(roles);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }
    }
}
