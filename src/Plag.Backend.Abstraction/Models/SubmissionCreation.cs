using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Plag.Backend.Models
{
    public class SubmissionCreation
    {
        [JsonPropertyName("setid")]
        public string SetId { get; set; }

        [JsonPropertyName("category")]
        public int? Category { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("files")]
        public ICollection<SubmissionFileCreation> Files { get; set; }

        public class SubmissionFileCreation
        {
            [JsonPropertyName("path")]
            public string FilePath { get; set; }

            [JsonPropertyName("name")]
            public string FileName { get; set; }

            [JsonPropertyName("content")]
            public string Content { get; set; }
        }
    }
}
