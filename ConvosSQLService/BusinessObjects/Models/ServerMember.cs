using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models
{
    public class ServerMember
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public User User { get; set; }
        public Guid ServerId { get; set; }

        public Server Server { get; set; }

        public string Nickname { get; set; }

        [Required]
        public DateTime JoinedAt { get; set; }

        public bool Muted { get; set; }

        public bool Deafened { get; set; }

        public bool Banned { get; set; }

        public ICollection<MemberRole> MemberRoles { get; set; }
        public ICollection<Invite> Invites { get; set; }
        public ICollection<InviteUsage> InvitesUsages { get; set; }
        public ICollection<Event> Events {  get; set; }

        public List<Quiz> CreatedQuizzes { get; set; }
        public List<QuizMember> ParticipatedQuizzes { get; set; }

    }
}
