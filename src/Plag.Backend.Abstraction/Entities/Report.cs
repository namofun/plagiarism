using System;

namespace Plag.Backend.Entities
{
    public class Report<TKey> where TKey : IEquatable<TKey>
    {
        public TKey SetId { get; set; }

        public int SubmissionA { get; set; }

        public int SubmissionB { get; set; }

        public TKey ExternalId { get; set; }

        public int TokensMatched { get; set; }

        public int BiggestMatch { get; set; }

        public double Percent { get; set; }

        public double PercentA { get; set; }

        public double PercentB { get; set; }

        public bool? Finished { get; set; }

        public byte[] Matches { get; set; }

        public bool? Justification { get; set; }

        public Models.Report ToModel()
        {
            return new Models.Report
            {
                BiggestMatch = BiggestMatch,
                SetId = SetId.ToString(),
                Id = ExternalId.ToString(),
                Matches = Matches,
                Pending = !Finished.GetValueOrDefault(),
                Percent = Percent,
                PercentA = PercentA,
                PercentB = PercentB,
                SubmissionA = SubmissionA,
                SubmissionB = SubmissionB,
                TokensMatched = TokensMatched,
                Justification = Justification,
            };
        }
    }
}
