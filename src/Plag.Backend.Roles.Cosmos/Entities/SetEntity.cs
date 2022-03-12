using System.Text.Json.Serialization;

namespace Xylab.PlagiarismDetect.Backend.Entities
{
    public class SetEntity : Models.PlagiarismSet
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = "sets";
    }
}
