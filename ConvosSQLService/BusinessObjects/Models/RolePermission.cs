using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class RolePermission
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }
        public bool IsGranted { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; }
        public Permission Permission { get; set; }
    }
}
