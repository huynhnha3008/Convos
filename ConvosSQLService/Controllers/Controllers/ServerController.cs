using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using BusinessObjects.DTOs;
using Newtonsoft.Json;
using StackExchange.Redis;
using BusinessObjects.DTOs.ServerMemberDto;
using BusinessObjects.QueryObject;
using Vonage.Conversations;

namespace Services.Controllers
{
    [Route("api/servers")]
    [ApiController]
    public class ServerController : ControllerBase
    {

        private readonly IServerService _serverService;

        private readonly IServerMemberService _serverMemberService;

        public ServerController( IServerService serverService, IServerMemberService serverMemberService)
        {
            _serverService = serverService;
            _serverMemberService = serverMemberService;
        }


        //* Idea: create server 
        [Authorize ]
        [HttpPost]
        public async Task<ActionResult<string>> CreateServer([FromForm] ServerCreateRequest server, IFormFile? iconFile)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (server.Type <= 0 || server.Type > 4)
            {
                return BadRequest("Only 4 server Type is accepted: 1. Default Server; 2. Study Group Server; 3. Peer Collaboration Server; 4. Class Server");
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var message = await _serverService.CreateAsync(server, iconFile, currentUserId);
                return Ok(message);
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


        //* idea: khi user click vào 1 server trong list sẽ lưu permission của currentUser vào redis
        // 5/12/24: tách redis ra đẻ tăng tốc độ return 
        [Authorize ]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetServerById(Guid id)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var server = await _serverService.GetServerChannelRoleResponseAsync(id, currentUserId);
                return Ok(server);
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





        [Authorize ]
        [HttpGet("{id}/roles")]
        public async Task<IActionResult> GetServerRoleHierarchyById(Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var roles = await _serverService.GetRoleHierarchyModelAsync(id);
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


        //* Idea: get all server cuar admin
        [Authorize ]
        [HttpGet]
        public async Task<IActionResult> GetAllServers([FromQuery] QueryServer query)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var servers = await _serverService.GetAllAsync(query); 
                return Ok(servers);
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


        //* Idea: láy tất cả server mà current user đã tham gia
        // 25/04/25: update checking permission for view channel (private channel)
        [Authorize ]
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetAllServersByUserId(Guid id)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var servers = await _serverService.GetAllServersByUserIdAsync(id);
                return Ok(servers);
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
        [HttpGet("categories/users/{id}")]
        public async Task<IActionResult> GetServersCategoriesChannelByUserId(Guid id)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var servers = await _serverService.GetAllServersCategoriesChannelByUserIdAsync(id);
                return Ok(servers);
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

        //* Idea: Update server
        [Authorize ]
        [HttpPut("{id:guid}"), Authorize ]
        public async Task<IActionResult> UpdateServer(Guid id, [FromForm] ServerUpdateRequest updateRequest, IFormFile? iconFile)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var server = await _serverService.UpdateAsync(id, currentUserId, updateRequest, iconFile);
                return Ok(server);
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

        //* Delete
        [Authorize ]
        [HttpDelete("{id:guid}"), Authorize ]
        public async Task<IActionResult> DeleteServer(Guid id)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var deletedServer = await _serverService.DeleteAsync(id, currentUserId);
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


        //* Idea: get all server's banned members
        [Authorize]
        [HttpGet("{id}/banned-members")]
        public async Task<IActionResult> GetBannedMembers(Guid id)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var members = await _serverMemberService.GetBannedMembersAsync(id);
                return Ok(members);
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


        //* Idea: get all server's members
        [Authorize ]
        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetServerMembers(Guid id, [FromQuery] QueryMember query)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var members = await _serverMemberService.GetAllAsync(id, query);
                if (members == null)
                {
                    return NotFound("No members is found");
                }
                return Ok(members);
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

        //* Idea : lấy thông tin của 1 member trong server 
        [Authorize ]
        [HttpGet("{id}/members/{member-id}")]
        public async Task<IActionResult> GetServerMember(Guid id, [FromRoute(Name = "member-id")] Guid memberId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var members = await _serverMemberService.GetByIdAsync(memberId);
                return Ok(members);
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


        //* Idea: leave server 
        [HttpPost("{id}/leave"), Authorize ]
        public async Task<IActionResult> LeaveServer(Guid id)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var member = await _serverMemberService.LeaveServerAsync(id, currentUserId);
                return Ok(member);
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

        //* Idea: kick member
        [Authorize ]
        [HttpPost("{id}/kick-member/{user-id}")]
        public async Task<IActionResult> KickMember(Guid id, [FromRoute(Name = "user-id")] Guid userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var member = await _serverMemberService.KickMemberAsync(id, userId, currentUserId);
                return Ok(member);
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


        //* Idea: ban 1 member hoặc hủy ban 1 member
        [Authorize ]
        [HttpPost("{id}/toggle-ban-member/{user-id}")]
        public async Task<IActionResult> ToggleBanMember(Guid id, [FromRoute(Name = "user-id")] Guid userId, [FromQuery] bool isBan)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                string messsage;
                if (isBan)
                {
                    messsage = await _serverMemberService.BanMemberAsync(id, userId, currentUserId);

                }
                else
                {
                    messsage = await _serverMemberService.UnBanMemberAsync(id, userId, currentUserId);
                }
                return Ok(messsage);
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

        [Authorize ]
        [HttpPost("{id}/toggle-mute-member/{user-id}")]
        public async Task<IActionResult> ToggleMuteMember(Guid id, [FromRoute(Name = "user-id")] Guid userId, [FromQuery] bool isMute)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                string message;
                if (isMute)
                {
                    message = await _serverMemberService.MuteMemberAsync(id, userId, currentUserId);
                }
                else
                {
                    message = await _serverMemberService.UnMuteMemberAsync(id, userId, currentUserId);
                }
                return Ok(message);
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


        [HttpPost("{id}/toggle-deafen-member/{user-id}"), Authorize ]
        public async Task<IActionResult> ToggleDeafenMember(Guid id, [FromRoute(Name = "user-id")] Guid userId, [FromQuery] bool isDeafen)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);

                string messsage;
                if (isDeafen)
                {
                    messsage = await _serverMemberService.DeafenMemberAsync(id, userId, currentUserId);
                }
                else
                {
                    messsage = await _serverMemberService.UnDeafenMemberAsync(id, userId, currentUserId);
                }
                return Ok(messsage);
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

        [Authorize ]
        [HttpPut("{id}/update-member-nickname/{user-id}")]
        public async Task<IActionResult> UpdateServerMember(Guid id, [FromRoute(Name = "user-id")] Guid userId, [FromQuery] ServerMemberUpdateRequest request)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                ServerMemberCreateRequest serverMemberCreateRequest = new ServerMemberCreateRequest
                {
                    NickName = request.NickName,
                    ServerId = id,
                    UserId = userId,
                };
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);

                var member = await _serverMemberService.UpdateAsync(serverMemberCreateRequest, currentUserId);
                if (member == null)
                {
                    return NotFound("Server or Member is not found");
                }
                return Ok(member);
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
