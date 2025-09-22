using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.UserDTO
{
    public class VerifyOtpDto
    {

        public UserRegister UserInfo { get; set; }

        [Required]
        public string OTPCodeEmail { get; set; }
        public string? OTPCodePhone { get; set; }
    }
}
