using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Service.NotificationService;

namespace Service.HubService.ChannelMessageHub
{
    public class ChannelMessageHub : Hub
    {
        private readonly INotificationService _notificationService;
        private static readonly Dictionary<string, HashSet<string>> _channelConnections = new();
        private static readonly object _lock = new();

        public ChannelMessageHub(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.Identity.Name;
            await Clients.Caller.SendAsync("Connected", userId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User.Identity.Name;
            lock (_lock)
            {
                foreach (var channel in _channelConnections)
                {
                    if (channel.Value.Remove(userId))
                    {
                        _notificationService.NotifyUserLeftChannel(channel.Key, userId).Wait();
                    }
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinChannel(string channelId)
        {
            var userId = Context.User.Identity.Name;
            lock (_lock)
            {
                if (!_channelConnections.ContainsKey(channelId))
                {
                    _channelConnections[channelId] = new HashSet<string>();
                }
                _channelConnections[channelId].Add(userId);
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, channelId);
            await _notificationService.NotifyUserJoinedChannel(channelId, userId);
        }

        public async Task LeaveChannel(string channelId)
        {
            var userId = Context.User.Identity.Name;
            lock (_lock)
            {
                if (_channelConnections.ContainsKey(channelId))
                {
                    _channelConnections[channelId].Remove(userId);
                }
            }
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);
            await _notificationService.NotifyUserLeftChannel(channelId, userId);
        }

        public async Task<List<string>> GetOnlineUsers(string channelId)
        {
            lock (_lock)
            {
                if (_channelConnections.TryGetValue(channelId, out var users))
                {
                    return users.ToList();
                }
                return new List<string>();
            }
        }

        public async Task ReceiveNotification(string channelId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, channelId);
        }
    }
}