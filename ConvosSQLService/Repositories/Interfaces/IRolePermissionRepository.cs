using BusinessObjects.Models;
using Services.Interfaces;

namespace Repositories.Interfaces
{
    public interface IRolePermissionRepository:IGenericRepository<RolePermission>
    {
        Task<List<RolePermission>> GetAllByRoleId(Guid roleId);
        Task<RolePermission> GetRolePermissionById(Guid roleId, Guid permissionId);
        Task UpdateRangeAsync(List<RolePermission> rolePermissions);
    }
}
