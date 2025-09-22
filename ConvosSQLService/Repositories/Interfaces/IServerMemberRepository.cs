using BusinessObjects.DTOs;
using BusinessObjects.Models;

namespace Services.Interfaces
{
    public interface IServerMemberRepository : IGenericRepository<ServerMember>
    {
        Task<ServerMember> FindByUserIdAndServerIdAsync(ServerMemberCreateRequest serverMemberCreateRequest);

        Task<ServerMember> GetByUserIdAndServerIdIncludeRolesPermissionsAsync(ServerMemberCreateRequest serverMemberCreateRequest);
        Task<ServerMember> GetSimpleByUserIdServerIdAsync(Guid  serverId, Guid userId);
        Task<ServerMember> GetByUserIdAndServerIdIncludeRoles(Guid userId, Guid serverId);
        Task<List<ServerMember>> GetAllAsync(Guid serverId);

        Task<List<ServerMember>> GetAllByRoleIdAsync(Guid roleId);

        Task<List<Guid>> GetAllServerIdByUserIdAsync(Guid userId);

        Task<ServerMember> BanMemberAsync(ServerMember member);

        Task<ServerMember> GetMemberIncludeUserAsync(Guid memberId);
    }
}
