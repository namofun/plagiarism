using System;

namespace Plag.Backend.Entities
{
    public class Report
    {
        public Guid Id { get; set; }

        public int SubmissionA { get; set; }

        public int SubmissionB { get; set; }

        public int TokensMatched { get; set; }

        public int BiggestMatch { get; set; }

        public double Percent { get; set; }

        public double PercentA { get; set; }

        public double PercentB { get; set; }

        public bool Pending { get; set; }

        public byte[] Matches { get; set; }
    }
}
