using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Dtos;
using Microsoft.AspNetCore.SignalR;
using Service.HubService.ChannelMessageHub;

namespace Service.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<ChannelMessageHub> _hubContext;

        public NotificationService(IHubContext<ChannelMessageHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyMessageReceived(string channelId, MessageDto message)
        {
            await _hubContext.Clients.Group(channelId).SendAsync("MessageReceived", message);
        }

        public async Task NotifyMessageEdited(string channelId, MessageDto message)
        {
            await _hubContext.Clients.Group(channelId).SendAsync("MessageEdited", message);
        }

        public async Task NotifyMessageDeleted(string channelId, string messageId)
        {
            await _hubContext.Clients.Group(channelId).SendAsync("MessageDeleted", messageId);
        }

        public async Task NotifyReactionAdded(string channelId, MessageDto message)
        {
            await _hubContext.Clients.Group(channelId).SendAsync("ReactionAdded", message);
        }

        public async Task NotifyReactionRemoved(string channelId, MessageDto message)
        {
            await _hubContext.Clients.Group(channelId).SendAsync("ReactionRemoved", message);
        }

        public async Task NotifyMessageRead(string channelId, string messageId, string userId)
        {
            await _hubContext.Clients.Group(channelId).SendAsync("MessageRead", messageId, userId);
        }

        public async Task NotifyUserJoinedChannel(string channelId, string userId)
        {
            await _hubContext.Clients.Group(channelId).SendAsync("UserJoined", channelId, userId);
        }

        public async Task NotifyUserLeftChannel(string channelId, string userId)
        {
            await _hubContext.Clients.Group(channelId).SendAsync("UserLeft", channelId, userId);
        }

        public async Task NotifyChannelMessageReceived(string channelId, MessageDto message)
        {
            await _hubContext.Clients.Group(channelId).SendAsync("ChannelMessageReceived", message);
        }

        public async Task NotifyMessagePinned(string channelId, MessageDto message)
        {
            await _hubContext.Clients.Group(channelId).SendAsync("MessagePinned", message);
        }

        public async Task NotifyMessageUnpinned(string channelId, MessageDto message)
        {
            await _hubContext.Clients.Group(channelId).SendAsync("MessageUnpinned", message);
        }
    }
}