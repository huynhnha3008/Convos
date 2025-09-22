using BusinessObjects.Models;

namespace Services.Interfaces
{
    public interface IInviteUsageRepository : IGenericRepository<InviteUsage>
    {
        Task<InviteUsage> GetInviteUsageByServerMemberIdAsync(Guid serverMemberId);
    }
}
