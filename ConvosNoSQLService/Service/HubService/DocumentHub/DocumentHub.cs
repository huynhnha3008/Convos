using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Service.HubService.DocumentHub
{
    public class DocumentHub : Hub<IDocumentHubClient>
    {
        public async Task JoinDocument(string documentId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, documentId);
        }

        public async Task LeaveDocument(string documentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, documentId);
        }
    }
} 