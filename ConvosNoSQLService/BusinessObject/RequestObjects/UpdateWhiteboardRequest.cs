using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
namespace BusinessObject.RequestObjects
{
    public class UpdateWhiteboardRequest
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("content")]
        [JsonConverter(typeof(JsonStringConverter))]
        public object Content { get; set; }

        [JsonPropertyName("excalidrawData")]
        [JsonConverter(typeof(JsonStringConverter))]
        public object ExcalidrawData { get; set; }

        [JsonPropertyName("attachments")]
        public List<IFormFile> Attachments { get; set; }
    }
}