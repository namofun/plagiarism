using System.Text.Json.Serialization;

namespace Plag.Backend.Entities
{
    public class SetEntity : Models.PlagiarismSet
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = "sets";
    }
}
