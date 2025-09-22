using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class RoleCreateRequest
    {
        public string Name { get; set; }

        public string Color { get; set; }

        public Guid ServerId { get; set; }
    }
}
