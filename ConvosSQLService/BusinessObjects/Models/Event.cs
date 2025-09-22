using BusinessObjects.DTOs.EventDto;
using BusinessObjects.DTOs.RoleDto;
using BusinessObjects.QueryObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class Event
    {
        public Guid Id { get; set; }
        public Guid CreatorId { get; set; } // servermemberId
        public Guid ServerId { get; set; }
        public Guid ChannelId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public Channel Channel { get; set; }
        public ServerMember Creator { get; set; }
        public Server Server { get; set; }
    }
}
