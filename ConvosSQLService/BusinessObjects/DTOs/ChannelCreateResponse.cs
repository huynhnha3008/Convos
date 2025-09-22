using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{

    public class ChannelCreateResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ChannelType Type { get; set; }
        public Guid ServerId { get; set; }
        public Guid CreatorId { get; set; }
        public Guid? CategoryId { get; set; }
        public int Position { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsPrivate { get; set; }
        public Category? Category { get; set; }
        public ICollection<ChannelRolePermission> ChannelRolePermissions { get; set; }
    }
}
