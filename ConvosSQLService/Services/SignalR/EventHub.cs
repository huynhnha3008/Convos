using Microsoft.AspNetCore.SignalR;
using Services.SignalR.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.SignalR
{
    public class EventHub : Hub<IEventHub>
    {

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


        public async Task CreateEvent(Guid serverId, string name)
        {
            await Clients.Group(serverId.ToString()).CreateEvent(serverId, name);
        }
        public async Task DeleteEvent(Guid serverId, string name)
        {
            await Clients.Group(serverId.ToString()).DeleteEvent(serverId, name);
        }

        public async Task UpdateEvent(Guid serverId, string name)
        {
            await Clients.Group(serverId.ToString()).UpdateEvent(serverId, name);
        }


    }
}
