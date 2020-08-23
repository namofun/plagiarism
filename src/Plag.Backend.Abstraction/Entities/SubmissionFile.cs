namespace Plag.Backend.Entities
{
    public class SubmissionFile
    {
        public int SubmissionId { get; set; }

        public int FileId { get; set; }

        public string FilePath { get; set; }
        
        public string FileName { get; set; }

        public string Content { get; set; }
    }
}
