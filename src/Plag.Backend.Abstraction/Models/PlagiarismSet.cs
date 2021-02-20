using System;
using System.Text.Json.Serialization;

namespace Plag.Backend.Models
{
    public class PlagiarismSet
    {
        [JsonPropertyName("setid")]
        public string Id { get; set; }

        [JsonPropertyName("create_time")]
        public DateTimeOffset CreateTime { get; set; }

        [JsonPropertyName("creator")]
        public int? UserId { get; set; }

        [JsonPropertyName("related")]
        public int? ContestId { get; set; }

        [JsonPropertyName("formal_name")]
        public string Name { get; set; }

        [JsonPropertyName("report_count")]
        public int ReportCount { get; set; }

        [JsonPropertyName("report_pending")]
        public int ReportPending { get; set; }

        [JsonPropertyName("submission_count")]
        public int SubmissionCount { get; set; }

        [JsonPropertyName("submission_failed")]
        public int SubmissionFailed { get; set; }

        [JsonPropertyName("submission_succeeded")]
        public int SubmissionSucceeded { get; set; }
    }
}
