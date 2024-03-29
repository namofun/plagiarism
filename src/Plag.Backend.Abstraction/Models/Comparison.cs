﻿using System.Text.Json.Serialization;

namespace Xylab.PlagiarismDetect.Backend.Models
{
    public class Comparison
    {
        [JsonPropertyName("reportid")]
        public virtual string Id { get; set; }

        [JsonPropertyName("submitid")]
        public int SubmissionIdAnother { get; set; }

        [JsonPropertyName("submit")]
        public string SubmissionNameAnother { get; set; }

        [JsonPropertyName("exclusive")]
        public int ExclusiveCategory { get; set; }

        [JsonPropertyName("state")]
        public virtual ReportState State { get; set; }

        [JsonPropertyName("tokens_matched")]
        public int TokensMatched { get; set; }

        [JsonPropertyName("biggest_match")]
        public int BiggestMatch { get; set; }

        [JsonPropertyName("percent")]
        public double Percent { get; set; }

        [JsonPropertyName("percent_self")]
        public double PercentSelf { get; set; }

        [JsonPropertyName("percent_another")]
        public double PercentIt { get; set; }

        [JsonPropertyName("justification")]
        public virtual ReportJustification Justification { get; set; }
    }
}
