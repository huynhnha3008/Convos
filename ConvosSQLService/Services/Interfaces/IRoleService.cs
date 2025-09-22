using BusinessObjects.DTOs;
using BusinessObjects.DTOs.RoleDto;
using BusinessObjects.Models;
using BusinessObjects.QueryObject;

namespace Services.Interfaces
{
    public interface IRoleService
    {

        Task<RoleCreateResponse> GetByIdAsync(Guid id);
        Task<string> RemoveMemberRole(Guid serverMemberId, Guid roleId, Guid userId);
        Task AssignDefaultEveryonePermissionsChannel(Guid channelId, Guid roleId); //
        Task AssignRolePermissionsChannel(Guid channelId, Guid roleId);  // channel's permission asssigning 
        Task AssignDefaultEveryonePermissions(Guid roleId); // server's permission asssigning
        Task<RoleCreateResponse> CreateAsync(RoleCreateRequest Role, Guid userId);

        Task<List<Guid>> GetAllRoleIdByServerIdAsync(Guid serverId);
        Task<IEnumerable<RoleCreateResponse>> GetAllByServerIdAsync(Guid serverId, QueryRole query);

        Task<string> ChangeRolePositionInServerAsync(Guid roleId, int newPosition, Guid serverId, Guid userId);

        Task<int> UpdatePermissionsToChannelRoleAsync(Guid roleId, List<UpdateChannelRolePermissionDTO> updateRoleDTOs, Guid channelId, Guid currentUserId);
        Task<string> UpdateAsync(Guid Id, Guid userId, RoleUpdateRequest roleUpdateRequest);

        Task<List<RoleCreateResponse>> SearchAsync(Guid serverId, QueryRole query);

        Task<string> AddMemberRole(Guid serverMemberId, Guid RoleId, Guid currentUserId);
        Task<string> AssignRoleToMemberWithoutChecking(Guid serverMemberId, Guid RoleId);

        Task AddRoleToChannel(Guid channelId, Guid roleId, Guid currentUserId);

        Task RemoveRoleFromChannel(Guid channelId, Guid roleId, Guid userId);
        Task<string> DeleteAsync(Guid id, Guid userId);
        Task<int> UpdatePermissionsToRoleAsync(Guid roleId, List<UpdateRolePermissionDTO> updateRoleDTOs, Guid currentUserId);

        Task<List<RoleCreateResponse>> GetAllByMemberIdAsync (Guid memberId);
    }
}
