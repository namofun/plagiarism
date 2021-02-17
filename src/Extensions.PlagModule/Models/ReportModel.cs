using Plag.Backend.Entities;

namespace SatelliteSite.PlagModule.Models
{
    public class ReportModel
    {
        public Report Report { get; }

        public CodeModel A { get; }

        public CodeModel B { get; }

        public ReportModel(Report report, CodeModel a, CodeModel b)
        {
            Report = report;
            A = a;
            B = b;
        }
    }
}
