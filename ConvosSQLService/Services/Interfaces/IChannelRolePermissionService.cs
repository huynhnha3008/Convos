using BusinessObjects.DTOs;
using BusinessObjects.Models;
namespace Services.Interfaces
{
    public interface IChannelRolePermissionService
    {
        Task<ChannelRolePermission> CreateAsync(ChannelRolePermissionCreateRequest request);
        Task UpdateAsync(Guid id, ChannelRolePermissionUpdateRequest request);
        Task<List<ChannelRolePermissionCreateResponse>> GetAllByChannelRoleId(Guid channelId, Guid roleId);
        Task DeleteAsync(Guid id);
        Task<ChannelRolePermissionCreateResponse> GetByIdAsync(Guid id);
    }
}
