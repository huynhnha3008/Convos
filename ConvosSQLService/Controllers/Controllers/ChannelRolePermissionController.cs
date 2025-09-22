using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Services.Controllers
{
    [ApiController]
    [Route("api/channel-role-permissions")]
    public class ChannelRolePermissionController : ControllerBase
    {
        private readonly IChannelRolePermissionService _channelRolePermissionService;

        public ChannelRolePermissionController(IChannelRolePermissionService channelRolePermissionService)
        {
            _channelRolePermissionService = channelRolePermissionService;
        }

        // POST: api/channel-role-permissions

        [Authorize ]
        [HttpPost]
        public async Task<IActionResult> CreateChannelRolePermission([FromBody] ChannelRolePermissionCreateRequest request)
        {
            try
            {
                var channelRolePermission = await _channelRolePermissionService.CreateAsync(request);
                return Ok(channelRolePermission);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize ]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var channelRolePermission = await _channelRolePermissionService.GetByIdAsync(id);
                return Ok(channelRolePermission);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize ]
        [HttpGet("channels/{channel-id}/roles/{role-id}")]
        public async Task<IActionResult> GetAllByChannelNRoleId([FromRoute(Name = "channel-id")] Guid channelId, [FromRoute(Name = "role-id")] Guid roleId)
        {
            try
            {
                var channelRolePermission = await _channelRolePermissionService.GetAllByChannelRoleId(channelId, roleId);
                return Ok(channelRolePermission);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize ]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromQuery] ChannelRolePermissionUpdateRequest request)
        {
            try
            {
                await _channelRolePermissionService.UpdateAsync(id, request);
                return NoContent();
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize ]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            try
            {
                await _channelRolePermissionService.DeleteAsync(id);
                return NoContent();
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
