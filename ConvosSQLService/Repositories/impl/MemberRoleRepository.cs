using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace Services.impl
{
    public class MemberRoleRepository : GenericRepository<MemberRole>, IMemberRoleRepository
    {
        private readonly ConvosDbContext _context;

        public MemberRoleRepository(ConvosDbContext context) : base(context) 
        {
            _context = context;
        }

        public async Task<List<MemberRole>> GetAllByMemberId(Guid serverMemberId)
        {
            return await _context.MemberRoles.Include(mr => mr.ServerMember)
                                           .Include(mr => mr.Role)
                                           .ThenInclude(r => r.RolePermissions)
                                           .ThenInclude(rp => rp.Permission)
                                           .Where(mr => mr.ServerMemberId.Equals(serverMemberId))
                                           .ToListAsync();
        }

        public async Task<MemberRole> GetByMemberIdAndRoleAsync(Guid memberId, Guid roleId)
        {
            return await _context.MemberRoles.FirstOrDefaultAsync(mr => mr.ServerMemberId.Equals(memberId) && mr.RoleId == roleId);
        }

        public async Task<List<MemberRole>> GetMembersByRoleIdAsync(Guid roleId)
        {
            return await _context.MemberRoles.Include(mr => mr.ServerMember)
                                             .Include(mr => mr.Role)
                                             .Where(mr =>mr.RoleId.Equals(roleId))
                                             .ToListAsync(); 
        }
    }
}
