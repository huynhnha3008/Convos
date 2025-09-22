using BusinessObjects.DTOs.ServerDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.ServerMemberDto
{
    public class MemberRoleResponse
    {
        public Guid roleId { get; set; }
        public string RoleName { get; set; }
        public string RoleColor { get; set; }

        public RoleDetailPermissionResponse permissions { get; set; }
    }
}
