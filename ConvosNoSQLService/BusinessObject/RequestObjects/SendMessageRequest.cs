using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Dtos;
using Microsoft.AspNetCore.Http;

namespace BusinessObject.RequestObjects
{
    public class SendMessageRequest
    {
        public string SenderId { get; set; }
        public string Content { get; set; }
        public string ReceiverId { get; set; }
        public string? ReplyToMessageId { get; set; }
        public List<IFormFile>? Attachments { get; set; }
    }
}