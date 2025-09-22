using BusinessObjects.DTOs.ServerDto;
using BusinessObjects.DTOs.UserDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.ServerMemberDto
{
    public class BannedMemberResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Avatar {  get; set; }
        public string Nickname { get; set; }

        public DateTime JoinedAt { get; set; }

        public bool Muted { get; set; }

        public bool Deafened { get; set; }

        public bool Banned { get; set; }

        public InviteUsageServerDetailResponse InvitesUsage { get; set; }
    }
}
