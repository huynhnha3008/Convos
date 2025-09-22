using BusinessObjects.Models;

namespace Services.Interfaces
{
    public interface IInviteRepository  : IGenericRepository<Invite>
    {
        Task<List<Invite>> GetAllAsync(Guid serverId);
        Task<Invite> GetByCodeAsync(string inviteCode);
        Task<List<Invite>> GetAllMemberInvitesAsync(Guid serverMemberId);
    }
}
