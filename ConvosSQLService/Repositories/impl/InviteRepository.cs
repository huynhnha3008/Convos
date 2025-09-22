using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace Services.impl
{
    public class InviteRepository : GenericRepository<Invite>, IInviteRepository
    {
        private readonly ConvosDbContext _context;
        public InviteRepository(ConvosDbContext convosDbContext): base(convosDbContext) 
        {
            _context = convosDbContext;
        }
     


        public async Task<Invite> GetByCodeAsync(string inviteCode)
        {
            try
            {
                return await _context.Invites
                                     .FirstOrDefaultAsync(i => i.Code.Equals(inviteCode));
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching the Invite by Id: " + ex.Message, ex);
            }
        }

      
        public async Task<List<Invite>> GetAllAsync(Guid serverId)
        {
            return await _context.Invites.Where(i => i.ServerId.Equals(serverId))
                                         .Include(i => i.Server)
                                        .Include(i => i.ServerMember)
                                        .ToListAsync();
        }

        public async Task<List<Invite>> GetAllMemberInvitesAsync(Guid serverMemberId)
        {
            return await _context.Invites.Where(i => i.CreatorId.Equals(serverMemberId))
                                         .Include(i => i.ServerMember)
                                         .ToListAsync();
        }
    }
}
