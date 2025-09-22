using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Dtos;
using BusinessObject.RequestObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Service.HubService.PrivateMessageHub;
using Service.PrivateMessageService;

namespace ConvosNoSQLWebApi.Controllers
{
    [ApiController]
    [Route("api/private-messages")]
    public class PrivateMessageController : ControllerBase
    {
        private readonly IPrivateMessageService _messageService;
        private readonly IHubContext<PrivateMessageHub> _hubContext;

        public PrivateMessageController(IPrivateMessageService messageService, IHubContext<PrivateMessageHub> hubContext)
        {
            _messageService = messageService;
            _hubContext = hubContext;
        }

        [HttpPost("send")]
        public async Task<ActionResult<PrivateMessageDto>> SendMessage([FromForm] SendMessageRequest request)
        {
            var message = await _messageService.SendMessageAsync(request.SenderId, request);
            return Ok(message);
        }

        [HttpPut("edit")]
        public async Task<ActionResult<PrivateMessageDto>> EditMessage([FromBody] EditMessageRequest request)
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

        [HttpGet("conversation/{otherUserId}")]
        public async Task<ActionResult<List<PrivateMessageDto>>> GetConversation(string senderId, string otherUserId, [FromQuery] int skip = 0, [FromQuery] int limit = 50)
        {
            var messages = await _messageService.GetConversationAsync(senderId, otherUserId, skip, limit);
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
        public async Task<ActionResult<PrivateMessageDto>> AddReaction(string userId, string messageId, [FromBody] string emoji)
        {

            var message = await _messageService.AddReactionAsync(userId, messageId, emoji);
            if (message == null)
                return NotFound();
            return Ok(message);
        }

        [HttpDelete("{messageId}/react")]
        public async Task<ActionResult<PrivateMessageDto>> RemoveReaction(string userId, string messageId, [FromBody] string emoji)
        {
            var message = await _messageService.RemoveReactionAsync(userId, messageId, emoji);
            if (message == null)
                return NotFound();
            return Ok(message);
        }

        [HttpPost("{messageId}/pin")]
        public async Task<ActionResult<PrivateMessageDto>> PinMessage(string userId, string messageId)
        {
            var message = await _messageService.PinMessageAsync(userId, messageId);
            if (message == null)
                return NotFound();
            return Ok(message);
        }

        [HttpPost("{messageId}/unpin")]
        public async Task<ActionResult<PrivateMessageDto>> UnpinMessage(string userId, string messageId)
        {
            var message = await _messageService.UnpinMessageAsync(userId, messageId);
            if (message == null)
                return NotFound();
            return Ok(message);
        }

        [HttpGet("pinned/{otherUserId}")]
        public async Task<ActionResult<List<PrivateMessageDto>>> GetPinnedMessages(string senderId, string otherUserId)
        {
            var messages = await _messageService.GetPinnedMessagesAsync(senderId, otherUserId);
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