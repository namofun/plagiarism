using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Plag.Backend.Entities
{
    public class PlagiarismSet
    {
        [JsonPropertyName("setid")]
        public string Id { get; set; }

        [JsonPropertyName("create_time")]
        public DateTimeOffset CreateTime { get; set; }

        [JsonPropertyName("formal_name")]
        public string Name { get; set; }

        [JsonPropertyName("report_count")]
        public int ReportCount { get; set; }

        [JsonPropertyName("report_pending")]
        public int ReportPending { get; set; }

        [JsonPropertyName("submissions")]
        public ICollection<Submission> Submissions { get; set; }
    }
}
