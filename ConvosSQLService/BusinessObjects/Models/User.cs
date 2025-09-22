using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();


        public string? Username { get; set; }


        public string? DisplayName { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }


        public string? Password { get; set; }


        public string? PhoneNumber { get; set; }

        public string? Avatar { get; set; }


        public Status? Status { get; set; }

        public UserRole? Role { get; set; }

        public string? Banner { get; set; }

        public string? Pronouns { get; set; }

        public string? About { get; set; }


        public string? Hashtag { get; set; }


        public DateTime? Birthdate { get; set; }


        public bool IsVerified { get; set; }


        public DateTime JoinedAt { get; set; }

        public ICollection<Friendship> RequestedFriendships { get; set; }

        public ICollection<Friendship> ReceivedFriendships { get; set; }

        public ICollection<ServerMember> ServerMembers { get; set; }

        public List<Subcription> Subcriptions { get; set; }


    }

    public enum Status
    {
        Online,
        Idle,
        DoNotDisturb,
        Invisible
    }
    public enum UserRole
    {
        Admin,
        User
    }
}