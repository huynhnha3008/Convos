using BusinessObjects.DTOs;
using BusinessObjects.DTOs.ServerDto;
using BusinessObjects.DTOs.ServersDTO;
using BusinessObjects.Models;
using BusinessObjects.QueryObject;
using Microsoft.AspNetCore.Http;

namespace Services.Interfaces
{
    public interface IServerService
        {
         
                Task<List<ServerBackupResponse>> GetAllServersCategoriesChannelByUserIdAsync(Guid userId);
                Task<List<ServerGetAllResponse>> GetAllAsync(QueryServer query);
                Task<List<ServerBackupResponse>> GetAllServersByUserIdAsync(Guid userId);
                Task<UserPermissionResponse> SetUserPermission(Guid userId, Guid serverId);
                Task<Server> GetByIdAsync(Guid id); // no use
                Task<string> UpdateAsync(Guid id, Guid userId, ServerUpdateRequest request, IFormFile? iconFile);
                Task<string> CreateAsync(ServerCreateRequest Server, IFormFile? IconFile, Guid currentUserId);
                Task<MemberServerDetailResponse> GetMemberServerDetail(Guid serverId, Guid userId); // include set currentUser permission to redis
                Task<List<ServerBackupResponse>> SearchAsync(QueryServer query);
                Task<string> DeleteAsync(Guid id, Guid userId);
                Task<ServerBackupResponse> GetServerDetailByIdAsync(Guid id);
                Task<Server> GetServerPermissionsAsync(Guid serverId);
                Task<ServerBackupResponse> GetServerNSetRedisPermissionsAsync(Guid userId, Guid serverId); // no use
                Task<ServerBackupResponse> GetServerChannelRoleResponseAsync(Guid serverId, Guid userId); // 6/12/24
                Task<RoleHierarchyModel> GetRoleHierarchyModelAsync(Guid serverId);
                Task<ServerBackupResponse> GetServersDetailCategoriesAndChannelByIdAsync(Guid id);
    }
}
