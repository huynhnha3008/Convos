using BusinessObjects.DTOs;
using BusinessObjects.QueryObject;

namespace Services.Interfaces
{
    public interface IInviteService
    {
        Task<InviteDetailResponse> CreateAsync(InviteCreateRequest inviteCreateRequest, Guid userId);
        Task<string> UpdateAsync(Guid id, InviteUpdateRequest request);
        Task<List<InviteDetailResponse>> GetServerInvitesAsync(Guid serverId, QueryInvite query);
        Task<InviteDetailResponse> GetByIdAsync(Guid id);
        Task<string> DeleteAsync(Guid id, Guid userId);
        Task<List<InviteDetailResponse>> SearchAsync(Guid serverId, QueryInvite query);
    }
}
