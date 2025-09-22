using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.RoleDto
{
    public class UpdateChannelRolePermissionRequest
    {
        public Guid roleId { get; set; }
        public Guid channelId { get; set; }

        public List<UpdateChannelRolePermissionDTO> rolePermissions { get; set; }

    }
}
