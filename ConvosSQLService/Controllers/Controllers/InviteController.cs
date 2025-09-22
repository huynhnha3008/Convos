using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Services.SignalR.Interfaces;
using Services;
using Services.Interfaces;
using Services.SignalR;
using Vonage.Conversations;
using Microsoft.AspNetCore.Authorization;
using BusinessObjects.DTOs;
using BusinessObjects.QueryObject;

namespace Services.Controllers
{
    [Route("api/invites")]
    [ApiController]
    public class InviteController : ControllerBase
    {
        private readonly IInviteService _inviteService;
        private readonly IInviteUsageService _inviteUsageService;
       

        public InviteController(IInviteService inviteService, IInviteUsageService inviteUsageService)
        {
            _inviteService = inviteService;
            _inviteUsageService = inviteUsageService;

        }
        [Authorize ]
        [HttpPost]
        public async Task<IActionResult> CreateInvite([FromBody] InviteCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var invite = await _inviteService.CreateAsync(request, currentUserId);
                var inviteUri = Url.Action("GetInvite", new { inviteId = invite.Id });
                return Created(inviteUri, invite);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }

        [Authorize ]
        [HttpPost("join")]
        public async Task<IActionResult> JoinByInviteCode([FromBody] InviteUsageCreateRequest inviteUsage)
        {
           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var inviteUsageResult= await _inviteUsageService.JoinServerByInviteCodeAsync(inviteUsage,currentUserId);
                return Ok(inviteUsageResult);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }

        [Authorize ]
        [HttpGet("server/{id}")]
        public async Task<IActionResult> GetAllServerInvites(Guid id, [FromQuery] QueryInvite query)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var inviteList = await _inviteService.GetServerInvitesAsync(id, query);
                return Ok(inviteList);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }

        [Authorize ]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvite(Guid id, [FromBody] InviteUpdateRequest request)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {

                var invite = await _inviteService.UpdateAsync(id, request);
                return Ok(invite);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }

        }

        [Authorize ]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInviteById(Guid id)
        {
           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var invite = await _inviteService.GetByIdAsync(id);
                return Ok(invite);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }

        }

        [Authorize ]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInviteById(Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var invite = await _inviteService.DeleteAsync(id, currentUserId);

                return NoContent();
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }
    }
}
