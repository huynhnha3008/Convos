using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.ChannelDto
{
    public class ChannelPermissionResponse
    {
        public Guid PermissionId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public string Description { get; set; }
    }
}
