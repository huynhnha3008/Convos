using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.EventDto
{
    public class EventCreateRequest
    {
        public Guid creatorId { get; set; } // servermemberId
        public Guid channelId { get; set; }
        [Required]
        public string title { get; set; }
        [Required]
        public string description { get; set; }
        [Required]
        public DateTime startAt { get; set; }
        [Required]
        public DateTime endAt { get; set; }

        public List<RoleIdObject> roleIds { get; set; }

    }


}
