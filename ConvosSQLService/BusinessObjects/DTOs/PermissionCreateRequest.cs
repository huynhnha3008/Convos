using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class PermissionCreateRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsServer { get; set; }
        public string Code { get; set; }
    }

    public class PermissionUpdateRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
    }

    public class PermissionCheckingRequest
    {
        public Guid ServerId { get; set; }
        public Guid UserId { get; set; }
        public Guid ChannelId { get; set; }
        public string PermissionCode { get; set; }
    }

}
