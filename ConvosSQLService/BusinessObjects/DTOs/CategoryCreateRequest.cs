
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class CategoryCreateRequest
    {
        public string Name { get; set; }
        public Guid ServerId { get; set; }
        public bool IsPrivate { get; set; }
    }
    public class CategoryUpdateRequest
    {
        public string? Name { get; set; }
        public bool IsPrivate { get; set; }
    }
}
