using System.Text.Json.Serialization;

namespace Plag.Backend.Models
{
    public class SubmissionFile
    {
        [JsonPropertyName("fileid")]
        public int FileId { get; set; }

        [JsonPropertyName("path")]
        public string FilePath { get; set; }

        [JsonPropertyName("name")]
        public string FileName { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
