using System;
using System.Collections.Generic;

namespace BusinessObject.Dtos
{
    public class DocumentDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string EditorJsData { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime? EditedTimestamp { get; set; }
        public string ChannelId { get; set; }
        public string CreatedBy { get; set; }
        public string LastEditedBy { get; set; }
        public bool IsEdited { get; set; }
        public bool Pinned { get; set; }
        public int Version { get; set; }
        public List<string> Collaborators { get; set; }
        public List<string> ReadBy { get; set; }
        public List<string> Tags { get; set; }
        public List<AttachmentDto> Attachments { get; set; }
    }
} 