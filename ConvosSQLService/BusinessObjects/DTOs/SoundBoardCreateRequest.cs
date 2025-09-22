using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class SoundBoardCreateRequest
    {
        public Guid ServerMemberId { get; set; }
        public Guid ServerId { get; set; }
        public string Name { get; set; }
        public string Emoji { get; set; }
        
    }
}
