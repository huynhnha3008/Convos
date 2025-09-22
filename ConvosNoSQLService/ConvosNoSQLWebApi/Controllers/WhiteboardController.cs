using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Dtos;
using BusinessObject.RequestObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Service.WhiteboardService;

namespace ConvosNoSQLWebApi.Controllers
{
    [Route("api/whiteboards")]
    public class WhiteboardController : ControllerBase
    {
        private readonly ILogger<WhiteboardController> _logger;
        private readonly IWhiteboardService _whiteboardService;

        public WhiteboardController(
            ILogger<WhiteboardController> logger,
            IWhiteboardService whiteboardService)
        {
            _logger = logger;
            _whiteboardService = whiteboardService;
        }

        [HttpPost]
        public async Task<ActionResult<WhiteboardDto>> CreateWhiteboard([FromBody] CreateWhiteboardRequest request)
        {
            try
            {

                var whiteboard = await _whiteboardService.CreateWhiteboardAsync(request.UserId, request);
                return Ok(whiteboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating whiteboard");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{whiteboardId}")]
        public async Task<ActionResult<WhiteboardDto>> GetWhiteboard(string whiteboardId)
        {
            try
            {
                var whiteboard = await _whiteboardService.GetWhiteboardByIdAsync(whiteboardId);
                if (whiteboard == null)
                    return NotFound();

                return Ok(whiteboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving whiteboard");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("channel/{channelId}")]
        public async Task<ActionResult<List<WhiteboardDto>>> GetChannelWhiteboards(
            string channelId,
            [FromQuery] int skip = 0,
            [FromQuery] int limit = 50)
        {
            try
            {
                var whiteboards = await _whiteboardService.GetChannelWhiteboardsAsync(channelId, skip, limit);
                return Ok(whiteboards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving channel whiteboards");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{whiteboardId}")]
        public async Task<ActionResult<WhiteboardDto>> UpdateWhiteboard(
            string whiteboardId,
            [FromBody] UpdateWhiteboardRequest request)
        {
            try
            {
                var whiteboard = await _whiteboardService.UpdateWhiteboardAsync(request.UserId, whiteboardId, request);
                if (whiteboard == null)
                    return NotFound();

                return Ok(whiteboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating whiteboard");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{whiteboardId}")]
        public async Task<ActionResult> DeleteWhiteboard(string whiteboardId, string userId)
        {
            try
            {

                var result = await _whiteboardService.DeleteWhiteboardAsync(userId, whiteboardId);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting whiteboard");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{whiteboardId}/collaborators/{collaboratorId}")]
        public async Task<ActionResult> AddCollaborator(string whiteboardId, string collaboratorId, string userId)
        {
            try
            {

                var result = await _whiteboardService.AddCollaboratorAsync(userId, whiteboardId, collaboratorId);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding collaborator");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{whiteboardId}/collaborators/{collaboratorId}")]
        public async Task<ActionResult> RemoveCollaborator(string whiteboardId, string collaboratorId, string userId)
        {
            try
            {

                var result = await _whiteboardService.RemoveCollaboratorAsync(userId, whiteboardId, collaboratorId);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing collaborator");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{whiteboardId}/pin")]
        public async Task<ActionResult> PinWhiteboard(string whiteboardId, string userId)
        {
            try
            {

                var result = await _whiteboardService.PinWhiteboardAsync(userId, whiteboardId);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pinning whiteboard");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{whiteboardId}/pin")]
        public async Task<ActionResult> UnpinWhiteboard(string whiteboardId, string userId)
        {
            try
            {

                var result = await _whiteboardService.UnpinWhiteboardAsync(userId, whiteboardId);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpinning whiteboard");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{whiteboardId}/read")]
        public async Task<ActionResult> MarkAsRead(string whiteboardId, string userId)
        {
            try
            {

                var result = await _whiteboardService.MarkAsReadAsync(userId, whiteboardId);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking whiteboard as read");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{whiteboardId}/tags/{tag}")]
        public async Task<ActionResult<WhiteboardDto>> AddTag(string whiteboardId, string tag, string userId)
        {
            try
            {

                var whiteboard = await _whiteboardService.AddTagAsync(userId, whiteboardId, tag);
                if (whiteboard == null)
                    return NotFound();

                return Ok(whiteboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding tag");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{whiteboardId}/tags/{tag}")]
        public async Task<ActionResult<WhiteboardDto>> RemoveTag(string whiteboardId, string tag, string userId)
        {
            try
            {

                var whiteboard = await _whiteboardService.RemoveTagAsync(userId, whiteboardId, tag);
                if (whiteboard == null)
                    return NotFound();

                return Ok(whiteboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing tag");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}