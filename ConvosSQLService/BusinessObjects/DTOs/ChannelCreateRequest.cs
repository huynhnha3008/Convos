
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class ChannelCreateRequest
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public Guid? ServerId { get; set; }
        public Guid? CategoryId { get; set; }
        public bool IsPrivate { get; set; }
    }
    public class ChannelUpdateRequest
    {
        public string? Name { get; set; }
        public bool IsPrivate { get; set; }
    }
}
