using BusinessObjects.DTOs.RoleDto;
using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.EventDto
{
    public class EventCreateResponse
    {
        public Guid id { get; set; }
        public Guid creatorId { get; set; } // servermemberId
        public Guid serverId { get; set; }
        public Guid channelId { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public bool status { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public DateTime startAt { get; set; }
        public DateTime endAt { get; set; }
    }


}
