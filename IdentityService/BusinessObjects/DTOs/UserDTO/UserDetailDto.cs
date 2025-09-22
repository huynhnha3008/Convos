using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.UserDTO
{
    public class UserDetailDto
    {
        public string DisplayName { get; set; }
        public string Username { get; set; }
        public string Hashtag { get; set; }
        public string About { get; set; }
        public DateTime JoinedAt { get; set; }
        public List<string> MutualFriends { get; set; }
        public List<string> MutualServers { get; set; }
    }
}
