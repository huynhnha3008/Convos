using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.ServersDTO
{
    public class ServerMemberDTO
    {
        public Guid MemberId { get; set; }
        public string UserName { get; set; }
        public string Nickname { get; set; }
        public string? Avatar { get; set; }
        public string Status { get; set; }
    }
}
