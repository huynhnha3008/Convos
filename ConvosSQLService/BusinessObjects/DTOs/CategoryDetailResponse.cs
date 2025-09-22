using BusinessObjects.DTOs.ServerDto;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class CategoryDetailResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid ServerId { get; set; }
        public int Position { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<CategoryChannelResponse> Channels { get; set; }

    }
}
