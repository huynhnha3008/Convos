using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public enum ChannelType
    {
        Text,
        Voice,
        Stage,
        Whiteboard,
        Docs
    }

    public class Channel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ChannelType Type { get; set; }
        public Guid ServerId { get; set; }
        public Guid CreatorId { get; set; } // servermemberId
        public Guid? CategoryId { get; set; }
        public int Position { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsPrivate { get; set; }
        public Category? Category { get; set; }
        public ICollection<ChannelRolePermission> ChannelRolePermissions { get; set; }
        public Server Server { get; set; }
        public ICollection<Event> Events { get; set; }
        public List<Quiz> quizs { get; set; }

    }
}
