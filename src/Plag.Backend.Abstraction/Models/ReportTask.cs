namespace Xylab.PlagiarismDetect.Backend.Models
{
    public class ReportTask
    {
        public string Id { get; }

        public string SetId { get; }

        public int SubmissionA { get; }

        public int SubmissionB { get; }

        public ReportTask(string reportid, string setid, int a, int b)
        {
            Id = reportid;
            SetId = setid;
            SubmissionA = a;
            SubmissionB = b;
        }
    }
}
