using BusinessObjects.DTOs;
using BusinessObjects.Models;

namespace Services.Interfaces
{
    public interface IPermissionRepository : IGenericRepository<Permission>
    {
        Task<Permission> GetByPermissionCode (string permissionCode);
    }
}

