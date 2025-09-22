using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.QuizDto
{
    public class QuizUpdateRequest
    {
        public Guid id { get; set; }
        public string? name { get; set; }
        public DateTime startTime { get; set; }
        public int? duration { get; set; }
        public string? description { get; set; }
    }
}
