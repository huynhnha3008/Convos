using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class RolePositionUpdateRequest
    {
        public Guid RoleId { get; set; }
        public int NewPosition { get; set; }
    }
}
