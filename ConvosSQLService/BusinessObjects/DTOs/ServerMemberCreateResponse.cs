


using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class ServerMemberCreateResponse
    {

            public Guid MemberId { get; set; }
            public User Member { get; set; }
            public Guid ServerId { get; set; }

            public string Nickname { get; set; }

            [Required]
            public DateTime JoinedAt { get; set; }

            public bool Muted { get; set; }

            public bool Deafened { get; set; }

            public bool Banned { get; set; }

            public ICollection<MemberRole> MemberRoles { get; set; }
            public ICollection<Invite> Invites { get; set; }

        
    }
}
