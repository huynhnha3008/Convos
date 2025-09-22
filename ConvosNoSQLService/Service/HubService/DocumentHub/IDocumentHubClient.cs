using System.Threading.Tasks;

namespace Service.HubService.DocumentHub
{
    public interface IDocumentHubClient
    {
        Task CollaboratorAdded(string documentId, string collaboratorId);
        Task CollaboratorRemoved(string documentId, string collaboratorId);
    }
} 