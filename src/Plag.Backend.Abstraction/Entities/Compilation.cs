using System.Text.Json.Serialization;

namespace Plag.Backend.Entities
{
    public class Compilation
    {
        [JsonPropertyName("submitid")]
        public string Id { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("tokens")]
        public byte[] Tokens { get; set; }
    }
}
