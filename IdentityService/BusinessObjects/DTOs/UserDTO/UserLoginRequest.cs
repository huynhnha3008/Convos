using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.UserDTO
{
    public class UserLoginRequest
    {
        public string keyLogin { get; set; }
        public string Password { get; set; }
    }
}
