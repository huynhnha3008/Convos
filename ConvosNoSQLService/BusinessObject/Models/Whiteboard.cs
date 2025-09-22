using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace BusinessObject.Models
{
    public class Whiteboard
    {
         [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("content")]
        public string Content { get; set; } = string.Empty;

        [BsonElement("excalidraw_data")]
        public string ExcalidrawData { get; set; } = string.Empty;

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [BsonElement("edited_timestamp")]
        public DateTime? EditedTimestamp { get; set; }

        [BsonElement("channel_id")]
        public string ChannelId { get; set; } = string.Empty;

        [BsonElement("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        [BsonElement("last_edited_by")]
        public string LastEditedBy { get; set; } = string.Empty;

        [BsonElement("is_pinned")]
        public bool Pinned { get; set; }

        [BsonElement("is_edited")]
        public bool IsEdited { get; set; }

        [BsonElement("attachments")]
        public List<Attachment>? Attachments { get; set; } = new List<Attachment>();

        [BsonElement("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [BsonElement("read_by")]
        public List<string> ReadBy { get; set; } = new List<string>();

        [BsonElement("collaborators")]
        public List<string> Collaborators { get; set; } = new List<string>();

        [BsonElement("version")]
        public int Version { get; set; } = 1;

        [BsonElement("tags")]
        public List<string> Tags { get; set; } = new List<string>();
    }
}