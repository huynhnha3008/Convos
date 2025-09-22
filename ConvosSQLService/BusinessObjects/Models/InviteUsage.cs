using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Models
{
    public class InviteUsage
    {
        public Guid Id { get; set; }
        public Guid InviteId { get; set; }

        public Guid ServerMemberId { get; set; }

        public DateTime UsedAt { get; set; }
        public ServerMember ServerMember { get; set; }
        public Invite Invite { get; set; }
    }
}
