using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class Quiz
    {
        public Guid Id { get; set; }
        public Guid ChannelId { get; set; }
        public Guid CreatorId { get; set; }
        public Guid ServerId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public string? Description { get; set; }
        public List<QuizMember> Participants { get; set; }   
        public ServerMember Creator {  get; set; }
        public Channel Channel { get; set; }
    }
}
