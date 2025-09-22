using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.UserDTO
{
    public class UserUpdateRequest
    {
        [Required]
        public Guid Id { get; set; } = Guid.NewGuid();


        public string? Username { get; set; }


        public string? DisplayName { get; set; }


        public string? Email { get; set; }




        public string? PhoneNumber { get; set; }




        public Status? Status { get; set; }



        public string? Banner { get; set; }

        public string? Pronouns { get; set; }

        public string? About { get; set; }





        public DateTime? Birthdate { get; set; }



    }
}
