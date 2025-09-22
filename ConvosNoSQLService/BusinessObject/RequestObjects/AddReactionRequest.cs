using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessObject.RequestObjects
{
    public class AddReactionRequest
    {
        public string memberId { get; set; } = string.Empty;
        public string emojiId { get; set; } = string.Empty;
    }
}