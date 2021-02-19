namespace Plag.Backend.Models
{
    public class ReportTask
    {
        public string Id { get; set; }

        public string SetId { get; set; }

        public int SubmissionA { get; set; }

        public int SubmissionB { get; set; }

        public ReportTask(string reportid, string setid, int a, int b)
        {
            Id = reportid;
            SetId = setid;
            SubmissionA = a;
            SubmissionB = b;
        }
    }
}
