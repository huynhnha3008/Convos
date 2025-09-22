using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class Subcription
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public SubcriptionType Type { get; set; }
        public double Price {  get; set; }
        public List<Feature> Features { get; set; }
        public User User { get; set; }

        public bool Status { get; set; } // truong hop thg user nay mua cai subcription khac khi 1 cai subcription van con han ...
    }


    public enum SubcriptionType
    {
        free,
        pro,
        premium
    }
}
