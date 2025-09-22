using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class MemberInviteResponse
    {
       
        public Guid MemberId { get; set; }

        public string Nickname { get; set; }

        public DateTime JoinedAt { get; set; }

        public bool Muted { get; set; }

        public bool Deafened { get; set; }

        public bool Banned { get; set; }

    }
}
