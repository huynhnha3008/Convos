using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessObject.Dtos
{
    public class ReactionDto
    {
        public string EmojiId { get; set; }
        public string Emoji { get; set; }
        public List<string> UserIds { get; set; }
    }
}