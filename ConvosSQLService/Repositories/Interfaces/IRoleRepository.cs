using BusinessObjects.Models;

namespace Services.Interfaces
{
    public interface IRoleRepository : IGenericRepository<Role>
    {

        Task<Role> FindByNameAndServerIdAsync(string roleName, Guid serverId);
        Task<List<Role>> GetRolesByServerIdAsync(Guid serverId);
        Task<List<Role>> GetSimpleRolesByServerIdAsync(Guid serverId);
        Task<List<Role>> GetRolesInServerByMemberId(Guid serverMemberId);
        Task<List<Role>> GetRolesInChannelByMemberId(Guid serverMemberId, Guid channelId);
    }
}
