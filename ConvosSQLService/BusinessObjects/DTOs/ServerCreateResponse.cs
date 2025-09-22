using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class ServerCreateResponse
    {
        public Guid OwnerId { get; set; }
        public string Name { get; set; }

        public string Icon { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int MembersCount { get; set; }
    }
}
