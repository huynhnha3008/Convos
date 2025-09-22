using Microsoft.AspNetCore.SignalR;
using Services.SignalR.Interfaces;

namespace Services.SignalR
{
    public class EmojiHub : Hub<IEmojiHub>
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


        public async Task CreateEmoji(Guid serverId, string name)
        {
            await Clients.Group(serverId.ToString()).CreateEmoji(serverId, name);
        }
        public async Task DeleteEmoji(Guid serverId, string name)
        {
            await Clients.Group(serverId.ToString()).DeleteEmoji(serverId, name);
        }

        public async Task UpdateEmoji(Guid serverId, string name)
        {
            await Clients.Group(serverId.ToString()).UpdateEmoji(serverId, name);
        }

    }
}
