using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Models
{
    public class Server
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OwnerId { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Icon { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        public ICollection<ServerMember> ServerMembers { get; set; }
        public ICollection<Role> Roles { get; set; }
        public ICollection<Invite> Invites { get; set; }
        public ICollection<Category> Categories { get; set; }
        public ICollection<Channel> Channels { get; set; }
        public ICollection<Emoji> Emojis { get; set; }
        public ICollection<SoundBoard> SoundBoards { get; set; }
        public ICollection<Event> Events { get; set; }

    }
}
