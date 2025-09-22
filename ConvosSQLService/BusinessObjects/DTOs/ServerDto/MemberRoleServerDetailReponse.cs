using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.ServerDto
{
    public class MemberRoleServerDetailReponse
    {
        public Guid roleId { get; set; }
        public string RoleName { get; set; }
        public List<ServerMemberDetailResponse> ServerMember { get; set; }

    }
}
