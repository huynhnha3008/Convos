using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace Services.impl
{
    public class ChannelRepository : GenericRepository<Channel>, IChannelRepository
    {
        private readonly ConvosDbContext _context;

        
        public ChannelRepository(ConvosDbContext context)  : base(context) 
        {
            _context = context;
        }

        public async Task<List<Channel>> GetAllByServerIdAsync(Guid serverId)
        {
            try
            {
                return await _context.Channels
                                     .Where(c => c.ServerId.Equals(serverId))
                                     .Include(x => x.ChannelRolePermissions)
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching Server Channels: " + ex.Message, ex);
            }
        }

        public async Task<List<Channel>> GetAllByCategoryIdAsync(Guid categoryId)
        {
            try
            {
                return await _context.Channels
                                     .Where(c => c.CategoryId.Equals(categoryId))
                                     .Include(x => x.ChannelRolePermissions)
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching Category Channels: " + ex.Message, ex);
            }
        }

        public async Task<Channel> GetSimpleChannelAsync(Guid channelId)
        {
            return await _context.Channels.FirstOrDefaultAsync(c => c.Id.Equals(channelId));
        }

        public async Task<Channel> GetByIdIncludeEventAsync(Guid channelId)
        {
            return await _context.Channels
                .Include(c =>c.Events)
                .FirstOrDefaultAsync(c => c.Id.Equals(channelId));
        }

        public async Task<List<Channel>> GetAllByRoleIdAsync(Guid roleId)
        {
            var channelRolePermissions = await _context.ChannelRolePermissions.Where(crp => crp.RoleId.Equals(roleId))
                                        .ToListAsync();
            HashSet<Channel> channels = new HashSet<Channel>();
            foreach (var crp in channelRolePermissions)
            {
                var c = await _context.Channels.FirstOrDefaultAsync(c => c.Id.Equals(crp.ChannelId));
                channels.Add(c);
            }
            return channels.ToList();
        }
    }
}
