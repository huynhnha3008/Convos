using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Dtos;
using BusinessObject.RequestObjects;

namespace Service.ChannelMessageService
{
     public interface IChannelMessageService
    {
        Task<MessageDto> SendMessageAsync(string senderId, SendChannelMessageRequest request);
        Task<MessageDto> EditMessageAsync(string userId, EditChannelMessageRequest request);
        Task<bool> DeleteMessageAsync(string userId, string messageId);
        Task<List<MessageDto>> GetChannelMessagesAsync(string memberId, string channelId, int skip = 0, int limit = 50);
        Task<bool> MarkAsReadAsync(string userId, string messageId);
        Task<MessageDto> AddReactionAsync(string userId, string messageId, string emoji);
        Task<MessageDto> RemoveReactionAsync(string userId, string messageId, string emoji);
        Task<MessageDto> PinMessageAsync(string userId, string messageId);
        Task<MessageDto> UnpinMessageAsync(string userId, string messageId);
        Task<List<MessageDto>> GetPinnedMessagesAsync(string channelId);
    }
}