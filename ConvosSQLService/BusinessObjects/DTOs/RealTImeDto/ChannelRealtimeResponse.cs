using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs.RealTImeDto
{
    public class ChannelRealtimeResponse
    {
        public Guid Id { get; set; }
        public string Name  { get; set; }
        public int Type { get; set; }
        public Guid CategoryId { get; set; }
        public int Position { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsPrivate { get; set; }

    }
}
