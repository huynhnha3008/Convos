using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Models;
using BusinessObject.RequestObjects;
using Microsoft.AspNetCore.Mvc;
using Service.MessageService;

namespace ConvosNoSQLWebApi.Controllers
{
    [ApiController]
    [Route("api/messages")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllMessages()
        {
            var messages = await _messageService.GetAllMessagesAsync();
            return Ok(messages);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessage(string id)
        {
            var message = await _messageService.GetMessageByIdAsync(id);
            if (message == null) return NotFound();

            return Ok(message);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage([FromBody] Message message)
        {
            var addedMessage = await _messageService.CreateMessageAsync(message);

            return CreatedAtAction(nameof(GetMessage), new { id = addedMessage.Id }, addedMessage);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMessage(string id, [FromBody] Message message)
        {
            var updatedMessage = await _messageService.UpdateMessageAsync(id, message);
            return Ok(updatedMessage);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(string id)
        {
            var deletedMessage = await _messageService.DeleteMessageAsync(id);
            return Ok(deletedMessage);
        }

        [HttpPost("{messageId}/reactions")]
        public async Task<IActionResult> AddReaction(string messageId, [FromBody] AddReactionRequest addReactionRequest)
        {
            var updatedMessage = await _messageService.AddReactionAsync(messageId, addReactionRequest.memberId, addReactionRequest.emojiId);

            return Ok(updatedMessage);
        }
    }
}