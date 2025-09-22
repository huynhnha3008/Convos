using BusinessObjects.Models;

namespace Services.Interfaces
{
    public interface IChannelRolePermissionRepository : IGenericRepository<ChannelRolePermission>
    {
        Task<ChannelRolePermission> GetByChannelRolePermissionAsync(Guid channelId, Guid roleId, Guid permissionId);
        Task<List<ChannelRolePermission>> GetAllRolePermissionsAsync(Guid channelId);
        Task<List<ChannelRolePermission>> GetChannelRolePermissionsByRoleIdAndChannelId(Guid roleId,Guid ChannelId);
        Task UpdateRangeAsync(List<ChannelRolePermission> channelRolePermissions);
    }
}
