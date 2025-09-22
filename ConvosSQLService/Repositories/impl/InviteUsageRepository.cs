using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace Services.impl
{
    public class InviteUsageRepository : GenericRepository<InviteUsage>, IInviteUsageRepository
    {
        private readonly ConvosDbContext _convosDbContext;

        public InviteUsageRepository (ConvosDbContext convosDbContext) : base (convosDbContext)
        {
            _convosDbContext = convosDbContext;
        }

        public async Task<InviteUsage> GetInviteUsageByServerMemberIdAsync(Guid serverMemberId)
        {
            return await _convosDbContext.InviteUsages.Include(iu => iu.Invite)
                                                      .FirstOrDefaultAsync(iu => iu.ServerMemberId.Equals(serverMemberId));
        }
    }
}
