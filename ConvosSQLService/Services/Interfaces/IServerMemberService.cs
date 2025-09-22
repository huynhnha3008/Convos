using BusinessObjects.DTOs;
using BusinessObjects.DTOs.ServerMemberDto;
using BusinessObjects.DTOs.UserDto;
using BusinessObjects.QueryObject;

namespace Services.Interfaces
{
    public interface IServerMemberService
    {
        Task<string> UpdateAsync(ServerMemberCreateRequest serverMemberCreateRequest, Guid userId);
        Task<List<ServerMemberResponse>> GetAllAsync(Guid serverId, QueryMember query);

        Task<ServerMemberResponse> GetServerMemberResponseAsync(Guid serverId, Guid serverMemberId);
        Task<ServerMemberResponse> GetByIdAsync(Guid id);
        Task<UserDetailDto> GetMemberUserDetailAsync(Guid currentUserId, Guid memberId);

        Task<List<BannedMemberResponse>> GetBannedMembersAsync(Guid serverId);

        Task<string> LeaveServerAsync(Guid serverId, Guid userId);
        Task<string> KickMemberAsync(Guid serverId, Guid targetUserId, Guid currentUserId );
        Task<string> BanMemberAsync(Guid serverId, Guid targetUserId, Guid currentUserId);
        Task<string> MuteMemberAsync(Guid serverId, Guid targetUserId, Guid currentUserId);
        Task<string> DeafenMemberAsync(Guid serverId, Guid targetUserId, Guid currentUserId);

        Task<string> UnBanMemberAsync(Guid serverId, Guid targetUserId, Guid currentUserId);
        Task<string> UnMuteMemberAsync(Guid serverId, Guid targetUserId, Guid currentUserId);
        Task<string> UnDeafenMemberAsync(Guid serverId, Guid targetUserId, Guid currentUserId);
    }
}
