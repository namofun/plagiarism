using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Plag.Backend.Models
{
    public class Submission
    {
        [JsonPropertyName("submitid")]
        public string Id { get; set; }

        [JsonPropertyName("setid")]
        public string SetId { get; set; }

        [JsonPropertyName("category")]
        public int? Category { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("max_percent")]
        public double MaxPercent { get; set; }

        [JsonPropertyName("token_produced")]
        public bool? TokenProduced { get; set; }

        [JsonPropertyName("upload_time")]
        public DateTimeOffset UploadTime { get; set; }

        [JsonPropertyName("files")]
        public IReadOnlyCollection<SubmissionFile> Files { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }
    }
}
