using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class MemberActCreateRequest
    {

        public bool? IsMuted { get; set; }
        public bool? IsDeafned { get; set; }
        public bool? IsBanned { get; set; }
    }
}
