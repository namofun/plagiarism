using System;
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

        [JsonPropertyName("state")]
        public string ProvisioningState { get; set; }

        [JsonPropertyName("matches")]
        public byte[] Matches { get; set; }

        [JsonPropertyName("justification")]
        public string Justification { get; set; }

        [JsonPropertyName("shared")]
        public bool Shared { get; set; }

        public static string GetJustificationName(bool? value)
        {
            return !value.HasValue ? "Unspecified" : value.Value ? "Claimed" : "Ignored";
        }

        public static bool? GetJustificationValue(string description)
        {
            return description?.ToLower() switch
            {
                "unspecified" => default(bool?),
                "claimed" => true,
                "ignored" => false,
                _ => throw new FormatException("Unknown justification value"),
            };
        }

        public static string GetProvisioningStateName(bool? value)
        {
            return !value.HasValue ? "Pending" : value.Value ? "Finished" : "Analyzing";
        }

        public static bool? GetProvisioningStateValue(string description)
        {
            return description?.ToLower() switch
            {
                "pending" => default(bool?),
                "finished" => true,
                "analyzing" => false,
                _ => throw new FormatException("Unknown provisioning state value"),
            };
        }
    }
}
