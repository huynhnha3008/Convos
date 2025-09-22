using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace BusinessObject.RequestObjects
{
    public class UpdateDocumentRequest
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
            [JsonPropertyName("userId")]
        public string UserId { get; set; }
        
        [JsonPropertyName("content")]
        [JsonConverter(typeof(JsonStringConverter))]
        public object Content { get; set; }
        
        [JsonPropertyName("editorJsData")]
        [JsonConverter(typeof(JsonStringConverter))]
        public object EditorJsData { get; set; }
        
        [JsonPropertyName("attachments")]
        public List<IFormFile> Attachments { get; set; }
    }
} 