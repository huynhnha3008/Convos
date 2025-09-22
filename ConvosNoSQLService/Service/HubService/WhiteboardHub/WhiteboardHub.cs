using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Service.HubService.WhiteboardHub
{
    public class WhiteboardHub : Hub<IWhiteboardHubClient>
    {
        public async Task JoinWhiteboard(string whiteboardId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, whiteboardId);
        }

        public async Task LeaveWhiteboard(string whiteboardId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, whiteboardId);
        }
    }
} 