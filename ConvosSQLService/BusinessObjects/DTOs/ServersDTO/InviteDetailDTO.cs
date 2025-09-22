using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.ServersDTO
{
    public class InviteDetailDTO
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string CreatorName { get; set; }
        public int MaxUses { get; set; }
        public int Uses { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
