using BusinessObjects.QueryObject;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models
{
    public class Role
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        public string Color { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool Mentionable { get; set; }
        public int Position { get; set; }

        public Guid ServerId { get; set; }

        public Server Server { get; set; }
        public virtual ICollection<RolePermission> RolePermissions { get; set; }
        public ICollection<MemberRole> MemberRoles { get; set; }
        public ICollection<ChannelRolePermission> ChannelRolePermissions { get; set; }

    }
}
