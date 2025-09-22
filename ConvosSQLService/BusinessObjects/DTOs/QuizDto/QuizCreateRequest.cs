using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.QuizDto
{
    public class QuizCreateRequest
    {
        public Guid channelId { get; set; }

        [Required]
        public string name { get; set; }

        public DateTime startTime { get; set; }

        [Required]
        public int duration { get; set; }
        public string? description { get; set; }
    }
}
