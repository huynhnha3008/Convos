using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.EventDto
{
    public class EventUpdateRequest
    {

        public string? title { get; set; }
        public string? description { get; set; }

        public DateTime startAt { get; set; }

        public DateTime endAt { get; set; }
    }
}
