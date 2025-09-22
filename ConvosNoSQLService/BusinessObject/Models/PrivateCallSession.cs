using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BusinessObject.Models
{
    public class PrivateCallSession
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("call_id")]
        public string CallId { get; set; } = Guid.NewGuid().ToString();
        
        [BsonElement("caller_id")]
        public string CallerId { get; set; } = string.Empty;

        [BsonElement("receiver_id")]
        public string ReceiverId { get; set; } = string.Empty;

        [BsonElement("call_type")]
        public string CallType { get; set; } = "voice";
        
        [BsonElement("start_time")]
        public DateTime StartTime { get; set; } = DateTime.Now;

        [BsonElement("end_time")]
        public DateTime? EndTime { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "initiated"; 

        [BsonElement("quality_metrics")]
        public string QualityMetrics { get; set; } = string.Empty; 

        [BsonElement("offer")]
        public string Offer { get; set; } = string.Empty;

        [BsonElement("answer")]
        public string Answer { get; set; } = string.Empty;

        [BsonElement("ice_candidates")]
        public string IceCandidates { get; set; } = string.Empty; 

        [BsonElement("device_info")]
        public string DeviceInfo { get; set; } = string.Empty; 

        [BsonElement("disconnected_reason")]
        public string DisconnectedReason { get; set; } = string.Empty;

        [BsonElement("notification_log")]
        public string NotificationLog { get; set; } = "[]";
    }
}