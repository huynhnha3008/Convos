using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.ServerDto
{

    public class RoleServerDetailReponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string Color { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool Mentionable { get; set; }

        public int Position { get; set; }

        public ICollection<ServerMemberDetailResponse> ServerMembers { get; set; }
        public RoleDetailPermissionResponse permissions { get; set; }

    }
}
