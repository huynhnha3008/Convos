using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Services.Controllers
{
    [Route("api/server-members")]
    [ApiController]
    public class ServerMemberController : ControllerBase
    {
        private readonly IServerMemberService _serverMemberService;

        public ServerMemberController(IServerMemberService serverMemberService)
        {
            _serverMemberService = serverMemberService;
        }


        [Authorize ]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMemberProfile(Guid id)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var server = await _serverMemberService.GetMemberUserDetailAsync(currentUserId, id);
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
    }
}
