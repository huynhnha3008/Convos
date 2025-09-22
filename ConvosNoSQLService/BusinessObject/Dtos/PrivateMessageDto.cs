using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessObject.Dtos
{
    public class PrivateMessageDto
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public bool IsEdited { get; set; }
        public List<AttachmentDto> Attachments { get; set; }
        public List<ReactionDto> Reactions { get; set; }
        public RepliedMessageDto? RepliedMessage { get; set; }
        public bool IsPinned { get; set; }
        public DateTime? PinnedAt { get; set; }
        public string PinnedBy { get; set; }
    }
}