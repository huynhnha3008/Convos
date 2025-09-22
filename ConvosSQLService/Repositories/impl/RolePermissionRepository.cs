using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Services.impl;

namespace Repositories.impl
{
    public class RolePermissionRepository : GenericRepository<RolePermission>, IRolePermissionRepository
    {
        private readonly ConvosDbContext _context;

        public RolePermissionRepository(ConvosDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<RolePermission>> GetAllByRoleId(Guid roleId)
        {
            return await _context.RolePermissions.Where(rp => rp.RoleId.Equals(roleId)).ToListAsync();
        }

        public async Task<RolePermission> GetRolePermissionById(Guid roleId,Guid permissionId)
        {
            return await _context.RolePermissions.FirstOrDefaultAsync(rp => rp.RoleId.Equals(roleId) && rp.PermissionId.Equals(permissionId));
        }

        public async Task UpdateRangeAsync(List<RolePermission> rolePermissions)
        {
            // Attach entities to the context if they are not already being tracked
            foreach (var rolePermission in rolePermissions)
            {
                _context.Entry(rolePermission).State = EntityState.Modified;
            }

            // Save all changes to the database
            await _context.SaveChangesAsync();
        }
    }
}
