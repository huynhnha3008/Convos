using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BusinessObject.Models
{
    public class Reaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string MemberId { get; set; } = string.Empty;
        public string MessageId { get; set; } = string.Empty;
        [BsonElement("emoji")]
        public string Emoji { get; set; } = string.Empty;
        [BsonElement("user_ids")]
        public List<string> UserIds { get; set; } = new List<string>();
    }
}