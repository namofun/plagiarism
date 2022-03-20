using Newtonsoft.Json;
using System;

namespace Xylab.PlagiarismDetect.Backend.Models
{
    public class SetStatistics
    {
        [JsonProperty("setid")]
        public string Id { get; set; }

        [JsonProperty("report_count")]
        public long ReportCount { get; set; }

        [JsonProperty("report_pending")]
        public long ReportPending { get; set; }

        [JsonProperty("submission_count")]
        public int SubmissionCount { get; set; }

        [JsonProperty("submission_failed")]
        public int SubmissionFailed { get; set; }

        [JsonProperty("submission_succeeded")]
        public int SubmissionSucceeded { get; set; }

        public void Merge(SetStatistics right)
        {
            ArgumentNullException.ThrowIfNull(right, nameof(right));
            if (Id != right.Id) throw new ArgumentException("Should be same ID.");

            ReportCount += right.ReportCount;
            ReportPending += right.ReportPending;
            SubmissionCount += right.SubmissionCount;
            SubmissionSucceeded += right.SubmissionSucceeded;
            SubmissionFailed += right.SubmissionFailed;
        }
    }
}
