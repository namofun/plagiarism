namespace Plag.Backend.Services
{
    public class GstReportService : IReportService
    {
        public Matching Generate(Submission subA, Submission subB)
        {
            return GSTiling.Compare(subA, subB, subA.Language.MinimalTokenMatch);
        }
    }
}
