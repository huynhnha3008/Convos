
namespace Services.SignalR.Interfaces
{
    public interface ICategoryHub
    {
        Task DeleteCategory(Guid serverId, string categoryName);
        Task CreateCategory(Guid serverId, string categoryName);
        Task UpdateCategory(Guid serverId, string categoryName);
        Task AlertToServer(Guid serverId, string categoryName);
    }
}
