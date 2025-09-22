using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class ServerMemberUpdateRequest
    {
        [Required]
        public string NickName { get; set; }
    }
}
