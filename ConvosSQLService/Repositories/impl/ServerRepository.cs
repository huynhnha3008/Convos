using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace Services.impl
{
    public class ServerRepository : GenericRepository<Server>, IServerRepository
    {
        private readonly ConvosDbContext _context;

        public ServerRepository(ConvosDbContext context) : base(context) 
        {
            _context = context;
        }

        

        public async Task<Server> GetServerAsync(Guid serverId)
        {
            return await _context.Servers
                .Include(s => s.Categories)
                    .ThenInclude(c => c.Channels)
                .Include(s => s.Channels)
                .Include(s => s.ServerMembers)
                    .ThenInclude(s => s.User)
                .Include(s => s.Emojis)
                .FirstOrDefaultAsync(s => s.Id.Equals(serverId));
        }

        public async Task<Server> GetServerIncludeCateChannelAsync(Guid serverId)
        {
            return await _context.Servers
                 .Include(s => s.Categories)
                    .ThenInclude(c =>c.Channels)
                 .Include(s => s.Channels)
                 .FirstOrDefaultAsync(s => s.Id.Equals(serverId));
        }

        public async Task<Server> GetServerIncludeChannelAsync(Guid serverId)
        {
            return await _context.Servers
          .Include(s => s.Channels)
          .FirstOrDefaultAsync(s => s.Id.Equals(serverId));
        }

        public async Task<Server> GetServerIncludeMembersAsync(Guid serverId)
        {
            return await _context.Servers
                .Include(s =>s.ServerMembers)
                    .ThenInclude(sm => sm.User)
                .FirstOrDefaultAsync(s => s.Id.Equals(serverId));

        }

        public async Task<Server> GetServerIncludeMembersChannelsAsync(Guid serverId)
        {
            return await _context.Servers
                 .Include(s => s.ServerMembers)
                 .Include(s => s.Channels)
                 .FirstOrDefaultAsync(s => s.Id.Equals(serverId));
        }

        public async Task<Server> GetServerIncludeMembersCateChannelsAsync(Guid serverId)
        {
            return await _context.Servers
                 .Include(s => s.ServerMembers)
                 .Include(s => s.Categories)
                        .ThenInclude(c=> c.Channels)
                 .FirstOrDefaultAsync(s => s.Id.Equals(serverId));
        }

        public async Task<Server> GetServerIncludePermissionsAsync(Guid serverId)
        {
            
            return await  _context.Servers
                 .Include(s => s.ServerMembers)
                   
                .Include(s => s.Roles)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)

                 .Include(s => s.Roles)
                    .ThenInclude(r => r.MemberRoles)
                    .ThenInclude(mr => mr.ServerMember)
                    .ThenInclude(sm => sm.User)
                .FirstOrDefaultAsync(s => s.Id.Equals(serverId));

        }

        public async Task<Server> GetServerIncludeRolesMemberAsync(Guid serverId)
        {
            return await _context.Servers
                .Include(s => s.ServerMembers)
                .Include(s =>s.Roles)
                .FirstOrDefaultAsync(s => s.Id.Equals(serverId));

        }

        public async Task<Server> GetServerOnlyAsync(Guid serverId)
        {
            return await _context.Servers.FirstOrDefaultAsync(s => s.Id.Equals(serverId));
        }

        public async Task UpdateServerRolePositionsAsync(Guid serverId, List<Role> updatedRoles)
        {
            var server = await _context.Servers.Include(s => s.Roles)
                                               .FirstOrDefaultAsync(s => s.Id == serverId);

            if (server == null) throw new Exception("Server not found!");

            foreach (var updatedRole in updatedRoles)
            {
                var role = server.Roles.FirstOrDefault(r => r.Id == updatedRole.Id);
                if (role != null)
                {
                    role.Position = updatedRole.Position;
                }
            }
            await _context.SaveChangesAsync();
        }

  
    
    }
}
