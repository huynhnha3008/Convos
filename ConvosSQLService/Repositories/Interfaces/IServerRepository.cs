using BusinessObjects.Models;

namespace Services.Interfaces
{
    public interface IServerRepository : IGenericRepository<Server>
    {
        Task UpdateServerRolePositionsAsync(Guid serverId, List<Role> updatedRoles);
        Task<Server> GetServerIncludePermissionsAsync(Guid serverId);
        Task<Server> GetServerAsync (Guid serverId);

        Task<Server> GetServerOnlyAsync (Guid serverId);
        Task<Server>  GetServerIncludeMembersChannelsAsync(Guid serverId);
        Task<Server> GetServerIncludeMembersCateChannelsAsync(Guid serverId);

        Task<Server> GetServerIncludeMembersAsync(Guid serverId);

        Task<Server> GetServerIncludeRolesMemberAsync (Guid serverId);

        Task<Server> GetServerIncludeChannelAsync (Guid serverId);
        Task<Server> GetServerIncludeCateChannelAsync (Guid serverId);
    }
}
