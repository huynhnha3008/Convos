using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Services.SignalR.Interfaces;

namespace Services.SignalR
{
    public class VoiceChannelHubs : Hub<IVoiceChannelHubs>
    {
        private static readonly ConcurrentDictionary<string, ConcurrentBag<string>> GroupConnections = new();

        public async Task JoinServer(string serverName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, serverName);
            var connections = GroupConnections.GetOrAdd(serverName, _ => new ConcurrentBag<string>());
            connections.Add(Context.ConnectionId);
        }
        public async Task LeaveVoiceChannel(string channelId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);
            if (GroupConnections.TryGetValue(channelId, out var connections))
            {
                connections = new ConcurrentBag<string>(connections.Except(new[] { Context.ConnectionId }));
                if (connections.IsEmpty)
                    GroupConnections.TryRemove(channelId, out _);
                else
                    GroupConnections[channelId] = connections;
            }
        }

        public async Task OnLeaveVoiceChannel(string channelId, string username)
        {
            await LeaveVoiceChannel(channelId);
            await Clients.Group(channelId).OnLeaveVoiceChannel(channelId, username);
        }

        public async Task AlertToChannelParticipants(string channelId, string username)
        {
            await JoinServer(channelId);
            await Clients.Group(channelId).AlertToChannelParticipants(channelId, username);
        }

    }
}
