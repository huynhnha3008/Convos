using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models
{
    public class MemberRole
    {
        public Guid ServerMemberId { get; set; }

        public virtual ServerMember ServerMember { get; set; }

        public Guid RoleId { get; set; }

        public Role Role { get; set; }
    }
}
