using System.Text.Json.Serialization;

namespace Plag.Backend.Models
{
    public class Compilation
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("tokens")]
        public byte[] Tokens { get; set; }
    }
}
