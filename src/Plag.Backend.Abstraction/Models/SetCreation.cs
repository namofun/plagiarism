using System.Text.Json.Serialization;

namespace Plag.Backend.Models
{
    public class SetCreation
    {
        [JsonPropertyName("creator")]
        public int? UserId { get; set; }

        [JsonPropertyName("related")]
        public int? ContestId { get; set; }

        [JsonPropertyName("formal_name")]
        public string Name { get; set; }
    }
}
