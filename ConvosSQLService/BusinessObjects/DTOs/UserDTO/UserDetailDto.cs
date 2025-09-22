using BusinessObjects.DTOs.RoleDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.UserDto
{
    public class UserDetailDto
    {
        public Guid userId {  get; set; }
        public string DisplayName { get; set; }
        public string Username { get; set; }
        public string Hashtag { get; set; }
        public string About { get; set; }
        public string Status { get; set; }
        public string Banner {  get; set; }
        public DateTime JoinedAt { get; set; }
        public List<string> MutualFriends { get; set; }
        public List<string> MutualServers { get; set; }
        public List<RoleDTO> roles { get; set; }
    }
}
