using BusinessObjects.DTOs;
using BusinessObjects.QueryObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Controllers.Controllers
{
    [Route("api/emojisql")]
    [ApiController]
    public class EmojiController : ControllerBase
    {
        private readonly IEmojiService _emojiService;
        private readonly IServerService _serverService;

        public EmojiController(IEmojiService emojiService, IServerService serverService)
        {

            _emojiService = emojiService;
            _serverService = serverService;
        }

        [Authorize ]
        [HttpPost]
        public async Task<ActionResult<EmojiCreateResponse>> CreateEmoji([FromForm] EmojiCreateRequest request, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);

                var emo = await _emojiService.CreateAsync(request, currentUserId,imageFile);

                return CreatedAtAction(nameof(GetEmojiById), new { id = emo.Id }, emo);
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
        public async Task<ActionResult> GetAllServerEmojis(Guid id, [FromQuery] QueryEmoji query)
        {
           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var emo = await _emojiService.GetAllAsync(id,query);
                
                return Ok(emo);
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
        public async Task<ActionResult> GetEmojiById(Guid id)
        {
           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var emo = await _emojiService.GetByIdAsync(id);
            
                return Ok(emo);
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
        [HttpPut]
        public async Task<IActionResult> UpdateServerEmoji([FromQuery] EmojiUpdateRequest request,IFormFile? imageFile)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var updatedEmo = await _emojiService.UpdateAsync(request, currentUserId,imageFile);
                return Ok(updatedEmo);
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
        public async Task<IActionResult> DeleteServerEmoji(Guid id)
        {
           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var emo = await _emojiService.DeleteAsync(id, currentUserId);
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
