using Microsoft.AspNetCore.SignalR;
using Services.SignalR.Interfaces;
using System.Collections.Concurrent;

namespace Services.SignalR
{
    public class InviteHub : Hub<IInviteHub>
    {
        private static readonly ConcurrentDictionary<Guid, List<string>> _serverMembers = new();

        public async Task JoinGroup(string serverId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, serverId);
            var connections = _serverMembers.GetOrAdd(Guid.Parse(serverId), _ => new List<string>());
            connections.Add(Context.ConnectionId);
            Console.WriteLine($"Client {Context.ConnectionId} joined group {serverId}");
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                await Clients.All.SendMessageAsync($"Client connected: {Context.ConnectionId}");
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during connection: {ex.Message}");
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception != null)
            {
                Console.WriteLine($"Client disconnected with error: {exception.Message}");
            }
            else
            {
                Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
            }

            await base.OnDisconnectedAsync(exception);
        }


        public async Task AlertToServer(Guid serverId, string username)
        {
            await JoinGroup(serverId.ToString());
            await Clients.Group(serverId.ToString()).AlertToServer(serverId, username);
        }

        public async Task UpdateMemberList(Guid serverId, Guid userId)
        {
            var members = _serverMembers.GetValueOrDefault(serverId, new List<string>());
            await Clients.Group(serverId.ToString()).UpdateMemberList();
        }
        public async Task CreateInvite(Guid serverId, string code)
        {
            await Clients.Group(serverId.ToString()).CreateInvite(serverId, code);
        }

        public async Task DeleteInvite(Guid serverId, string code)
        {
            await Clients.Group(serverId.ToString()).DeleteInvite(serverId, code);
        }
    }

}
