using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class JoinRoomRequest
    {
        public Guid ServerId { get; set; }

        public Guid UserId { get; set; }
    }
}
