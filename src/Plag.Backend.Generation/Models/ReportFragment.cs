namespace Plag.Backend.Models
{
    public class ReportFragment
    {
        public int TokensMatched { get; set; }

        public int BiggestMatch { get; set; }

        public double Percent { get; set; }

        public double PercentA { get; set; }

        public double PercentB { get; set; }

        public byte[] Matches { get; set; }
    }
}
