using BusinessObjects.DTOs;
using BusinessObjects.QueryObject;


namespace Services.Interfaces
{
    public interface IPermissionService
    {
        Task<UserPermissionResponse> GetUserPermissions( Guid serverId, Guid userId);
        Task<List<RolePermissionResponse>> GetUserChannelPermission(Guid userId, Guid serverId, Guid channelId);
        Task<List<RolePermissionResponse>> GetUserGlobalPermission(Guid userId, Guid serverId);
        Task<PermissionCreateResponse> GetByIdAsync(Guid id);
        Task<List<PermissionCreateResponse>> GetAllAsync(QueryPermission query);
        Task<PermissionCreateResponse> UpdateAsync(Guid id, PermissionUpdateRequest request);
        Task<PermissionCreateResponse> DeleteAsync(Guid id);
        Task<List<PermissionCreateResponse>> SearchAsync(QueryPermission query);
        Task<PermissionCreateResponse> CreateAsync(PermissionCreateRequest request);

        Task<bool> CheckPermission(Guid serverId, Guid userId, string permissionCode, Guid channelId);
    }
}
