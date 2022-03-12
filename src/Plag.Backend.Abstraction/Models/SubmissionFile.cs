using System.Text.Json.Serialization;

namespace Xylab.PlagiarismDetect.Backend.Models
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

        public SubmissionFile()
        {
        }

        public SubmissionFile(int fileId, SubmissionCreation.SubmissionFileCreation creation)
        {
            FileId = fileId;
            FileName = creation.FileName;
            Content = creation.Content;
            FilePath = creation.FilePath;
        }
    }
}
