using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class ServerMemberCreateRequest
    {
        [Required]
        public Guid ServerId { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string NickName { get; set; }
    }
}
