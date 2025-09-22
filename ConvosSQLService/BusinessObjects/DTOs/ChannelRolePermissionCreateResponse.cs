using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class ChannelRolePermissionCreateResponse
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ChannelId { get; set; }
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }
        public bool IsGranted { get; set; }

    }
}
