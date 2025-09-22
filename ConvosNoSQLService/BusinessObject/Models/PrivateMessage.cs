using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace BusinessObject.Models
{
    public class PrivateMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("content")]
        public string Content { get; set; } = string.Empty;

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [BsonElement("edited_timestamp")]
        public DateTime? EditedTimestamp { get; set; }

        [BsonElement("sender_id")]
        public string SenderId { get; set; } = string.Empty;

        [BsonElement("receiver_id")]
        public string ReceiverId { get; set; } = string.Empty;

        [BsonElement("conversation_id")]
        public string ConversationId { get; set; } = string.Empty;

        [BsonElement("reply_to_message_id")]
        public string ReplyToMessageId { get; set; } = string.Empty;

        [BsonElement("is_pinned")]
        public bool IsPinned { get; set; }

        [BsonElement("pinned_at")]
        public DateTime? PinnedAt { get; set; }

        [BsonElement("pinned_by")]
        public string PinnedBy { get; set; } = string.Empty;
        
        [BsonElement("is_edited")]
        public bool IsEdited { get; set; }

        [BsonElement("attachments")]
        public List<Attachment>? Attachments { get; set; } = new List<Attachment>();

        [BsonElement("reactions")]
        public List<Reaction>? Reactions { get; set; } = new List<Reaction>();

        [BsonElement("is_deleted")]
        public bool IsDeleted { get; set; }

        [BsonElement("read_by")]
        public List<string> ReadBy { get; set; } = new List<string>();
    }
}