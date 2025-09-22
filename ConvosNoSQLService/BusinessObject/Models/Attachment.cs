using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace BusinessObject.Models
{
    public class Attachment
    {
        [BsonElement("file_name")]
        public string FileName { get; set; } = string.Empty;

        [BsonElement("file_type")]
        public string FileType { get; set; } = string.Empty;

        [BsonElement("file_size")]
        public long FileSize { get; set; }

        [BsonElement("file_url")]
        public string FileUrl { get; set; } = string.Empty;
    }
}