using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class RoleInChannelPermissions
    {
        public Guid channelId {  get; set; }
        public List<RolePermissionResponse> permissions { get; set; }
    }
}
