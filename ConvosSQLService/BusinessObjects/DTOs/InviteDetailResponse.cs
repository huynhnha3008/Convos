
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class InviteDetailResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; }

        public Guid ServerId { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int MaxUses { get; set; }
        public int Uses { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public MemberInviteResponse ServerMember { get; set; }
    }
}
