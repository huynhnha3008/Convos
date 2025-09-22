using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.UserDTO
{
    public class FriendRequestDto
    {
        public Guid UserId { get; set; }

        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Hashtag { get; set; }
        public string? Avatar { get; set; }
        public string? Status { get; set; }
        public string? Banner { get; set; }
        public string? Pronouns { get; set; }
        public string? About { get; set; }
        public DateTime? Birthdate { get; set; }
    }
}
