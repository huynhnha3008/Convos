using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class UserPermissionResponse
    {
        public Guid userId { get; set; }
        public Guid serverId { get; set; }
        public List<RolePermissionResponse> permissions { get; set; }

        public List<ChannelPermission> channelPermissions { get; set; }
    }
}
