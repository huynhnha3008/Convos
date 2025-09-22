using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("QuizMember")]
    public class QuizMember
    {
        public Guid Id {  get; set; }
        public int Score { get; set; }
        public Guid ParticipantId { get; set; } // server member
        public Guid QuizId { get; set; }
        public DateTime? SubmittedTime { get; set; }
        public QuizStatus Status { get; set; }
        public ServerMember Participant { get; set; }
        public Quiz Quiz { get; set; }
    }

    public enum QuizStatus
    {
        in_progress,
        submitted,
        graded
    }
}
