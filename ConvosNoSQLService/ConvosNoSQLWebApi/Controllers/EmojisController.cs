using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Models;
using Microsoft.AspNetCore.Mvc;
using Service.EmojiService;

namespace ConvosNoSQLWebApi.Controllers
{
    [ApiController]
    [Route("api/emojis")]
    public class EmojisController : ControllerBase
    {
        private readonly IEmojiService _emojiService;

        public EmojisController(IEmojiService emojiService)
        {
            _emojiService = emojiService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmojis()
        {
            var emojis = await _emojiService.GetAllEmojisAsync();

            return Ok(emojis);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmoji(string id)
        {
            var emoji = await _emojiService.GetEmojiByIdAsync(id);
            if (emoji == null) return NotFound();

            return Ok(emoji);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmoji([FromBody] Emoji emoji)
        {
            var createdEmoji = await _emojiService.CreateEmojiAsync(emoji);

            return CreatedAtAction(nameof(GetEmoji), new { id = createdEmoji.Id }, createdEmoji);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmoji(string id, [FromBody] Emoji emoji)
        {
            var updatedEmoji = await _emojiService.UpdateEmojiAsync(id, emoji);
            return Ok(updatedEmoji);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmoji(string id)
        {
            var deletedEmoji = await _emojiService.DeleteEmojiAsync(id);
            return Ok(deletedEmoji);
        }
    }
}