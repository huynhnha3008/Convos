using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Models;

namespace Service.MessageService
{
    public interface IMessageService
    {
        Task<Message> GetMessageByIdAsync(string id);
        Task<IEnumerable<Message>> GetAllMessagesAsync();
        Task<Message> CreateMessageAsync(Message message);
        Task<Message> UpdateMessageAsync(string id, Message updatedMessage);
        Task<Message> DeleteMessageAsync(string id);
        Task<Message> AddReactionAsync(string messageId, string memberId, string emojiId);
    }
}