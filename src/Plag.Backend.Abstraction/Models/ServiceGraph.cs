using System.Text.Json.Serialization;

namespace Xylab.PlagiarismDetect.Backend.Models
{
    public class ServiceVertex
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("incl")]
        public int Inclusive { get; set; }

        [JsonPropertyName("excl")]
        public int Exclusive { get; set; }

        [JsonPropertyName("lang")]
        public string Language { get; set; }
    }

    public class ServiceEdge
    {
        [JsonPropertyName("u")]
        public int U { get; set; }

        [JsonPropertyName("v")]
        public int V { get; set; }
    }
}
