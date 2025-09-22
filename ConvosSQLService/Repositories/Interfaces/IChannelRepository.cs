using BusinessObjects.Models;

namespace Services.Interfaces
{
    public interface IChannelRepository :IGenericRepository<Channel>
    {
        Task<List<Channel>> GetAllByServerIdAsync(Guid serverId);
        Task<List<Channel>> GetAllByCategoryIdAsync(Guid categoryId);
        Task<Channel> GetSimpleChannelAsync(Guid channelId);
        Task<Channel> GetByIdIncludeEventAsync(Guid channelId);
        Task<List<Channel>> GetAllByRoleIdAsync(Guid roleId);

    }
}
