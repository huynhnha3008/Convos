using MimeKit.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class Emoji
    {
        public Guid Id { get; set; }
        public Guid ServerMemberId { get; set; }
        public Guid ServerId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public Server Server { get; set; }
        
    }
}
