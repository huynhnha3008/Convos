using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Dtos;
using BusinessObject.RequestObjects;
using Microsoft.AspNetCore.Mvc;
using Service.ChannelMessageService;

namespace ConvosNoSQLWebApi.Controllers
{
    [ApiController]
    [Route("api/channel-messages")]
    public class ChannelMessageController : ControllerBase
    {
        private readonly IChannelMessageService _messageService;

        public ChannelMessageController(IChannelMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost("send")]
        public async Task<ActionResult<MessageDto>> SendMessage([FromForm] SendChannelMessageRequest request)
        {
            var message = await _messageService.SendMessageAsync(request.MemberId, request);
            return Ok(message);
        }

        [HttpPut("edit")]
        public async Task<ActionResult<MessageDto>> EditMessage([FromBody] EditChannelMessageRequest request)
        {
            var message = await _messageService.EditMessageAsync(request.SenderId, request);
            if (message == null)
                return NotFound();
            return Ok(message);
        }

        [HttpDelete("{messageId}")]
        public async Task<ActionResult> DeleteMessage(string messageId, string senderId)
        {
            var result = await _messageService.DeleteMessageAsync(senderId, messageId);
            if (!result)
                return NotFound();
            return NoContent();
        }

        [HttpGet("conversation/{channelId}")]
        public async Task<ActionResult<List<MessageDto>>> GetChannelMessages(string memberId, string channelId, [FromQuery] int skip = 0, [FromQuery] int limit = 50)
        {
            var messages = await _messageService.GetChannelMessagesAsync(memberId, channelId, skip, limit);
            return Ok(messages);
        }

        [HttpPost("{messageId}/read")]
        public async Task<ActionResult> MarkAsRead(string senderId, string messageId)
        {
            var result = await _messageService.MarkAsReadAsync(senderId, messageId);
            if (!result)
                return NotFound();
            return NoContent();
        }

        [HttpPost("{messageId}/react")]
        public async Task<ActionResult<MessageDto>> AddReaction(string userId, string messageId, [FromBody] string emoji)
        {

            var message = await _messageService.AddReactionAsync(userId, messageId, emoji);
            if (message == null)
                return NotFound();
            return Ok(message);
        }

        [HttpDelete("{messageId}/react")]
        public async Task<ActionResult<MessageDto>> RemoveReaction(string userId, string messageId, [FromBody] string emoji)
        {
            var message = await _messageService.RemoveReactionAsync(userId, messageId, emoji);
            if (message == null)
                return NotFound();
            return Ok(message);
        }

        [HttpPost("{messageId}/pin")]
        public async Task<ActionResult<MessageDto>> PinMessage(string userId, string messageId)
        {
            var message = await _messageService.PinMessageAsync(userId, messageId);
            if (message == null)
                return NotFound();
            return Ok(message);
        }

        [HttpPost("{messageId}/unpin")]
        public async Task<ActionResult<MessageDto>> UnpinMessage(string userId, string messageId)
        {
            var message = await _messageService.UnpinMessageAsync(userId, messageId);
            if (message == null)
                return NotFound();
            return Ok(message);
        }

        [HttpGet("pinned/{channelId}")]
        public async Task<ActionResult<List<MessageDto>>> GetPinnedMessages(string channelId)
        {
            var messages = await _messageService.GetPinnedMessagesAsync(channelId);
            return Ok(messages);
        }

        // [HttpPost("join-conversation/{otherUserId}")]
        // public async Task<IActionResult> JoinConversation(string otherUserId)
        // {
        //     var userId = GetCurrentUserId();
        //     var conversationId = GetConversationId(userId, otherUserId);
        //     await _hubContext.Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        //     return Ok();
        // }

        // [HttpPost("leave-conversation/{otherUserId}")]
        // public async Task<IActionResult> LeaveConversation(string otherUserId)
        // {
        //     var userId = GetCurrentUserId();
        //     var conversationId = GetConversationId(userId, otherUserId);
        //     await _hubContext.Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
        //     return Ok();
        // }

        private string GetConversationId(string userId1, string userId2)
        {
            var sortedIds = new[] { userId1, userId2 }.OrderBy(id => id);
            return string.Join("_", sortedIds);
        }
        private string GetCurrentUserId()
        {
            throw new NotImplementedException();
        }
    }
}