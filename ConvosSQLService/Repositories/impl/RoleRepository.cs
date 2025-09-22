using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace Services.impl
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        private readonly ConvosDbContext _context;

        public RoleRepository(ConvosDbContext context) : base(context)
        {
            _context = context;
        }


        public async Task<Role> FindByNameAndServerIdAsync(string roleName, Guid serverId)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == roleName && r.ServerId == serverId);
        }


        public async Task<List<Role>> GetRolesByServerIdAsync(Guid serverId)
        {
            return await _context.Roles
                                 .Include(r =>r.ChannelRolePermissions)
                                 .Include(r =>r.RolePermissions)
                                 .Where(role => role.ServerId == serverId)
                                 .ToListAsync();
        }

        public async Task<List<Role>> GetRolesInChannelByMemberId(Guid serverMemberId, Guid channelId)
        {
            var roles = await _context.ServerMembers
                .Where(sm => sm.Id == serverMemberId)
                .SelectMany(sm => sm.MemberRoles.Select(mr => mr.Role))
                .Where(role => role.ChannelRolePermissions.Any(crp => crp.ChannelId == channelId))
                .ToListAsync();

            return roles;
        }


        public Task<List<Role>> GetRolesInServerByMemberId(Guid serverMemberId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Role>> GetSimpleRolesByServerIdAsync(Guid serverId)
        {
            return await _context.Roles.Where(r =>  r.ServerId == serverId)
                                       .ToListAsync(); 
        }
    }
}
