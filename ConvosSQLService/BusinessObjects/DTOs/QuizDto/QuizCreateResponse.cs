using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.QuizDto
{
    public class QuizCreateResponse
    {
        public Guid channelId { get; set; }
        public Guid creatorId { get; set; }
        public string name { get; set; }
        public string? description { get; set; }
        public string quizFile { get; set; }
        public DateTime createdDate { get; set; }
        public DateTime startTime { get; set; }
        public int duration { get; set; }
    }
}
