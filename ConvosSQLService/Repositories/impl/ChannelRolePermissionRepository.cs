using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace Services.impl
{
    public class ChannelRolePermissionRepository : GenericRepository<ChannelRolePermission>, IChannelRolePermissionRepository
    {
        private readonly ConvosDbContext _context;

        public ChannelRolePermissionRepository(ConvosDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<ChannelRolePermission>> GetAllRolePermissionsAsync(Guid channelId)
        {
            return await _context.ChannelRolePermissions.Where(crp => crp.ChannelId.Equals(channelId))
                .Include(crp => crp.Permission)
                .ToListAsync();
        }

        public async Task<ChannelRolePermission> GetByChannelRolePermissionAsync(Guid channelId, Guid roleId, Guid permissionId)
        {
            return await _context.ChannelRolePermissions
                .Include(crp => crp.Permission)
                .FirstOrDefaultAsync(crp => crp.ChannelId == channelId && crp.RoleId == roleId && crp.PermissionId == permissionId);
        }

        public async Task<List<ChannelRolePermission>> GetChannelRolePermissionsByRoleIdAndChannelId(Guid roleId, Guid channelId)
        {
            return await _context.ChannelRolePermissions
                .Include(crp => crp.Permission)
                .Where(crp => crp.ChannelId == channelId && crp.RoleId == roleId)
                .ToListAsync();
        }

        public async Task UpdateRangeAsync(List<ChannelRolePermission> channelRolePermissions)
        {
            // Attach entities to the context if they are not already being tracked
            foreach (var rolePermission in channelRolePermissions)
            {
                _context.Entry(rolePermission).State = EntityState.Modified;
            }

            // Save all changes to the database
            await _context.SaveChangesAsync();
        }

    }
}
