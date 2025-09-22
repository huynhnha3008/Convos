using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Dtos;
using BusinessObject.RequestObjects;

namespace Service.PrivateMessageService
{
    public interface IPrivateMessageService
    {
        Task<PrivateMessageDto> SendMessageAsync(string senderId, SendMessageRequest request);
        Task<PrivateMessageDto> EditMessageAsync(string userId, EditMessageRequest request);
        Task<bool> DeleteMessageAsync(string userId, string messageId);
        Task<List<PrivateMessageDto>> GetConversationAsync(string userId1, string userId2, int skip = 0, int limit = 50);
        Task<bool> MarkAsReadAsync(string userId, string messageId);
        Task<PrivateMessageDto> AddReactionAsync(string userId, string messageId, string emoji);
        Task<PrivateMessageDto> RemoveReactionAsync(string userId, string messageId, string emoji);
        Task<PrivateMessageDto> PinMessageAsync(string userId, string messageId);
        Task<PrivateMessageDto> UnpinMessageAsync(string userId, string messageId);
        Task<List<PrivateMessageDto>> GetPinnedMessagesAsync(string senderId, string otherUserId);
    }
}