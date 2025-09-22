using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.RoleDto
{
    public class UpdateChannelRolePermissionDTO
    {
        public Guid PermissionId { get; set; }
        public bool IsGranted { get; set; }
    }
}
