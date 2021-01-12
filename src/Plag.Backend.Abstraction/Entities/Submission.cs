using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Plag.Backend.Entities
{
    public class Submission
    {
        [JsonPropertyName("submitid")]
        public string Id { get; set; }

        [JsonPropertyName("setid")]
        public string SetId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("max_percent")]
        public double MaxPercent { get; set; }

        [JsonPropertyName("token_produced")]
        public bool? TokenProduced { get; set; }

        [JsonPropertyName("upload_time")]
        public DateTimeOffset UploadTime { get; set; }

        [JsonPropertyName("files")]
        public ICollection<SubmissionFile> Files { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }
    }
}
