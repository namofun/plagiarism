using Xylab.PlagiarismDetect.Frontend;

namespace Xylab.PlagiarismDetect.Backend.Services
{
    public class GstReportService : IReportService
    {
        public Matching Generate(Submission subA, Submission subB)
        {
            return GreedyStringTiling.Compare(subA, subB, subA.Language.MinimalTokenMatch);
        }
    }
}
