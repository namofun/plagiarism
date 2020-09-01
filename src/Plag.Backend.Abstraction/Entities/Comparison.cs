namespace Plag.Backend.Entities
{
    public class Comparison
    {
        public string Id { get; set; }

        public string SubmissionIdAnother { get; set; }

        public string SubmissionAnother { get; set; }

        public bool Pending { get; set; }

        public int TokensMatched { get; set; }

        public int BiggestMatch { get; set; }

        public double Percent { get; set; }

        public double PercentSelf { get; set; }

        public double PercentIt { get; set; }
    }
}
