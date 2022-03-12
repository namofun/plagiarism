using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xylab.PlagiarismDetect.Backend.Models
{
    public class Submission
    {
        [JsonPropertyName("setid")]
        public string SetId { get; set; }

        [JsonPropertyName("submitid")]
        public int Id { get; set; }

        [JsonPropertyName("externalid")]
        public string ExternalId { get; set; }

        [JsonPropertyName("exclusive_category")]
        public int ExclusiveCategory { get; set; }

        [JsonPropertyName("inclusive_category")]
        public int InclusiveCategory { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("max_percent")]
        public double MaxPercent { get; set; }

        [JsonPropertyName("token_produced")]
        public bool? TokenProduced { get; set; }

        [JsonPropertyName("upload_time")]
        public DateTimeOffset UploadTime { get; set; }

        [JsonPropertyName("files")]
        [JsonPropertyOrder(5)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IReadOnlyCollection<SubmissionFile> Files { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }
    }

    public class Vertex : Submission
    {
        [JsonPropertyName("comparisons")]
        [JsonPropertyOrder(10)]
        public IReadOnlyCollection<Comparison> Comparisons { get; set; }
    }
}
