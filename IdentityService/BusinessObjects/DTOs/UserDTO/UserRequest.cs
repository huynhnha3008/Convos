using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.UserDTO
{
    public class UserRequest
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Avatar { get; set; }
        public Status Status { get; set; }
        public string Role { get; set; }
        public string? Banner { get; set; }
        public string? Pronouns { get; set; }
        public string? About { get; set; }
        public string Hashtag { get; set; }
        public DateTime? Birthdate { get; set; }
        public bool IsVerified { get; set; }
        public DateTime JoinedAt { get; set; }
        public List<ServerDto> Servers { get; set; }
    }

}
