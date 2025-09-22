using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Dtos;

namespace Service.NotificationService
{
    public interface INotificationService
    {
        Task NotifyMessageReceived(string channelId, MessageDto message);
        Task NotifyMessageEdited(string channelId, MessageDto message);
        Task NotifyMessageDeleted(string channelId, string messageId);
        Task NotifyReactionAdded(string channelId, MessageDto message);
        Task NotifyReactionRemoved(string channelId, MessageDto message);
        Task NotifyMessageRead(string channelId, string messageId, string userId);
        Task NotifyUserJoinedChannel(string channelId, string userId);
        Task NotifyUserLeftChannel(string channelId, string userId);
        Task NotifyChannelMessageReceived(string channelId, MessageDto message);
        Task NotifyMessagePinned(string channelId, MessageDto message);
        Task NotifyMessageUnpinned(string channelId, MessageDto message);
    }
}