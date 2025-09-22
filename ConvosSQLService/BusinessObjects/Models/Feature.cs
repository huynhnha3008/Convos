using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class Feature
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public Guid SubcriptionId { get; set; }

        public Subcription Subcription { get; set; }    
    }
}
