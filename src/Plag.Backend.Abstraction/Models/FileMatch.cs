namespace Xylab.PlagiarismDetect.Backend.Models
{
    public class FileMatch
    {
        public int MatchingId { get; set; }

        public int FileA { get; set; }

        public int FileB { get; set; }

        public int ContentStartA { get; set; }

        public int ContentEndA { get; set; }

        public int ContentStartB { get; set; }

        public int ContentEndB { get; set; }
    }
}
