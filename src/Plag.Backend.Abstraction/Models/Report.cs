using System.Text.Json.Serialization;

namespace Plag.Backend.Models
{
    public class Report
    {
        [JsonPropertyName("setid")]
        public string SetId { get; set; }

        [JsonPropertyName("submitid_a")]
        public int SubmissionA { get; set; }

        [JsonPropertyName("submitid_b")]
        public int SubmissionB { get; set; }

        [JsonPropertyName("externalid")]
        public string Id { get; set; }

        [JsonPropertyName("tokens_matched")]
        public int TokensMatched { get; set; }

        [JsonPropertyName("biggest_match")]
        public int BiggestMatch { get; set; }

        [JsonPropertyName("percent")]
        public double Percent { get; set; }

        [JsonPropertyName("percent_a")]
        public double PercentA { get; set; }

        [JsonPropertyName("percent_b")]
        public double PercentB { get; set; }

        [JsonPropertyName("pending")]
        public bool Pending { get; set; }

        [JsonPropertyName("matches")]
        public byte[] Matches { get; set; }

        [JsonPropertyName("justification")]
        public bool? Justification { get; set; }
    }
}
