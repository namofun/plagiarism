using Plag.Frontend;

namespace Plag.Backend.Services
{
    /// <summary>
    /// The service to generate plagiarism detection report.
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Generate the report between two submissions.
        /// </summary>
        /// <param name="subA">The first submission.</param>
        /// <param name="subB">The second submission.</param>
        /// <returns>The matching report.</returns>
        Matching Generate(Submission subA, Submission subB);
    }
}
