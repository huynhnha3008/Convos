using BusinessObjects.DTOs.ServerDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class MemberServerDetailResponse
    {
        public ServerBackupResponse server {  get; set; }
        public List<RolePermissionResponse> userServerPermissions { get; set; }
        public List<RoleInChannelPermissions> userChannelPermissions { get; set; }

    }
}
