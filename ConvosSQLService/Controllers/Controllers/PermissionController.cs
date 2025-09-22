using BusinessObjects.DTOs;
using BusinessObjects.QueryObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Controllers.Controllers
{
    [Route("api/permissions")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> CreatePermission([FromBody] PermissionCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var createdPermission = await _permissionService.CreateAsync(request);
                return CreatedAtAction(nameof(GetPermissionById), new { id = createdPermission.Id }, createdPermission);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict( ex.Message);
            }

        }


        /// <summary>
        /// check user's permission in server
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        //[Authorize]

        [HttpPost("check")]
        public async Task<ActionResult> CheckPermission([FromBody] PermissionCheckingRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                return Ok(await _permissionService.CheckPermission(request.ServerId, request.UserId, request.PermissionCode, request.ChannelId));

            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }

        }

        [Authorize ]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPermissionById(Guid id)
        {
           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var permission = await _permissionService.GetByIdAsync(id);

                return Ok(permission);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [Authorize ]
        [HttpGet]
        public async Task<IActionResult> GetAllPermission([FromQuery] QueryPermission query)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var permissions = await _permissionService.GetAllAsync(query);

                return Ok(permissions);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatePermission(Guid id, [FromBody] PermissionUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var updatedPermission = await _permissionService.UpdateAsync(id, request);
                return Ok(updatedPermission);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePermission(Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var deletedPermission = await _permissionService.DeleteAsync(id);
                return NoContent();
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

    }
}
