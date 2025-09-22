using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace Services.impl
{
    public class PermissionRepository : GenericRepository<Permission>, IPermissionRepository
    {
        private readonly ConvosDbContext _context;

        public PermissionRepository(ConvosDbContext context) : base (context)
        {
            _context = context;
        }

        public async Task<Permission> GetByPermissionCode(string permissionCode)
        {
            var rs = await _context.Permissions.FirstOrDefaultAsync(p => p.Code.Equals(permissionCode));
            if (rs == null)
            {
                return new Permission();
            }
            return rs;
        }
    }
}
