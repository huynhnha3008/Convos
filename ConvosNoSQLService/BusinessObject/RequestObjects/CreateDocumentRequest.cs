using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace BusinessObject.RequestObjects
{
    public class CreateDocumentRequest
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

        [JsonPropertyName("channelId")]
        public string ChannelId { get; set; }

        [JsonPropertyName("attachments")]
        public List<IFormFile> Attachments { get; set; }
    }

    public class JsonStringConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                using var jsonDocument = JsonDocument.ParseValue(ref reader);
                return jsonDocument.RootElement.GetRawText();
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value is string stringValue)
            {
                writer.WriteStringValue(stringValue);
            }
            else
            {
                writer.WriteRawValue(value.ToString());
            }
        }
    }
}