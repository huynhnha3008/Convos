using BusinessObjects;
using BusinessObjects.DTOs;
using BusinessObjects.QueryObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Services.Controllers
{
    [Route("api/channels")]
    [ApiController]
    public class ChannelController : ControllerBase
    {
        private readonly IChannelService _channelService;

        public ChannelController(IChannelService channelService)
        {
            _channelService = channelService;
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateChannel([FromBody] ChannelCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var channel = await _channelService.CreateCustomAsync(request, currentUserId);

                return CreatedAtAction(nameof(GetChannelById), new { id = channel.Id }, channel);
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
        [HttpPost("{id}/change-channel-position")]
        public async Task<IActionResult> ChangeChannelPosition(Guid id, [FromQuery] ChannelUpdatePositionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var message = await _channelService.ChangePositionAsync(id, request.newPosition, currentUserId);

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


        /// <summary>
        /// test get all channels by roleId
        /// </summary>
        /// <param name="id"></param>
        /// <param name="query"></param>
        /// <returns></returns>


        [HttpGet("roles/{id}")]
        public async Task<IActionResult> GetAllByRoleId(Guid id)
        {
          
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var channelList = await _channelService.GetAllByRoleIdAsync(id);

                return Ok(channelList);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize ]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChannelById(Guid id)
        {
            var channel = await _channelService.GetByIdAsync(id);

            if (channel == null)
            {
                return NotFound("Channel not found.");
            }

            return Ok(channel);
        }

     

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChannel(Guid id,[Required] [FromBody] ChannelUpdateRequest request)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);

                var updatedChannel = await _channelService.UpdateAsync(id, currentUserId, request);

                return Ok(updatedChannel);
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChannel(Guid id)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var deletedChannel = await _channelService.GetByIdAsync(id);
                await _channelService.DeleteAsync(id, currentUserId);

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
        }
    }
}
