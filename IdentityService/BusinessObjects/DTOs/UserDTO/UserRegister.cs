using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.UserDTO
{
    public class UserRegister
    {



        [Required]
        public string Username { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }


        public string? PhoneNumber { get; set; }

        //public string? Avatar { get; set; }
        public string? Banner { get; set; }

        public string? Pronouns { get; set; }

        public string? About { get; set; }

        [Required]
        public DateTime? Birthdate { get; set; }


    }
}
