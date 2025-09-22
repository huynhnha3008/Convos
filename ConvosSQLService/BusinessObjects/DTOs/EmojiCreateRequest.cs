using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class EmojiCreateRequest
    {
        public Guid ServerMemberId { get; set; }
        public Guid ServerId { get; set; }
        [Required]
        public string Name { get; set; }
        
    }
}
