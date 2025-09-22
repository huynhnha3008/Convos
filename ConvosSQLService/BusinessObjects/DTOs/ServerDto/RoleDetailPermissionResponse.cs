using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.ServerDto
{
    public class RoleDetailPermissionResponse
    {
        public List<RolePermissionResponse> serverPermissions { get; set; } // server role permission

        public List<ChannelPermission> channelPermissions { get; set; } 
    }
}
