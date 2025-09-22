
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.ChannelDto
{
    public class ChannelRoleWithPermissionResponse
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public List<ChannelPermissionResponse> Permission { get; set; }

    }
}
