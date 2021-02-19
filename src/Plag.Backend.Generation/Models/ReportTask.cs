using System;

namespace Plag.Backend.Models
{
    public class ReportTask
    {
        public string Id { get; set; }

        public string SubmissionA { get; set; }

        public string SubmissionB { get; set; }

        public ReportTask(string id, string a, string b)
        {
            Id = id;
            SubmissionA = a;
            SubmissionB = b;
        }

        public ReportTask(Guid id, Guid a, Guid b)
        {
            Id = id.ToString();
            SubmissionA = a.ToString();
            SubmissionB = b.ToString();
        }

        public ReportTask(int id, int a, int b)
        {
            Id = id.ToString();
            SubmissionA = a.ToString();
            SubmissionB = b.ToString();
        }
    }
}
