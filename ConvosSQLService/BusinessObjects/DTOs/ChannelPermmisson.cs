using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class ChannelPermission
    {
        public Guid ChannelId { get; set; }
        public List<RolePermissionResponse> permissions { get; set; }

    }
}
