using System.Threading.Tasks;

namespace Service.HubService.WhiteboardHub
{
    public interface IWhiteboardHubClient
    {
        Task CollaboratorAdded(string whiteboardId, string collaboratorId);
        Task CollaboratorRemoved(string whiteboardId, string collaboratorId);
        Task WhiteboardUpdated(string whiteboardId, string userId, object update);
    }
} 