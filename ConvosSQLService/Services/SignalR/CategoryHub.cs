using Microsoft.AspNetCore.SignalR;
using Services.SignalR.Interfaces;
using System.Collections.Concurrent;

namespace Services.SignalR
{
    public class CategoryHub : Hub<ICategoryHub>
    {
        private static readonly ConcurrentDictionary<Guid, List<string>> _cateMember = new();

        public async Task JoinGroup(string serverId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, serverId);
            var connections = _cateMember.GetOrAdd(Guid.Parse(serverId), _ => new List<string>());
            connections.Add(Context.ConnectionId);
            Console.WriteLine($"Client {Context.ConnectionId} joined group {serverId}");
        }
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }


        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }


        public async Task CreateCategory(Guid serverId, string name)
        {
            await Clients.Group(serverId.ToString()).CreateCategory(serverId, name);
        }
        public async Task DeleteCategory(Guid serverId, string name)
        {
            await Clients.Group(serverId.ToString()).DeleteCategory(serverId, name);
        }

        public async Task UpdateCategory(Guid serverId, string name)
        {
            await Clients.Group(serverId.ToString()).UpdateCategory(serverId, name);
        }

        public async Task AlertToServer(Guid serverId, string name)
        {
            await JoinGroup(serverId.ToString());
            await Clients.Group(serverId.ToString()).AlertToServer(serverId, name);
        }
    }
}
