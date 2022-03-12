using System.Text.Json.Serialization;

namespace Xylab.PlagiarismDetect.Backend.Models
{
    public class ServiceVersion
    {
        [JsonPropertyName("fronend_version")]
        public string FrontendVersion { get; set; }

        [JsonPropertyName("backend_version")]
        public string BackendVersion { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }
    }
}
