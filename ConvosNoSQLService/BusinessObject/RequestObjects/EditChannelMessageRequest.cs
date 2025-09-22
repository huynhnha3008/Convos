using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessObject.RequestObjects
{
    public class EditChannelMessageRequest
    {
        public string SenderId { get; set; }
        public string MessageId { get; set; }
        public string NewContent { get; set; }
    }
}