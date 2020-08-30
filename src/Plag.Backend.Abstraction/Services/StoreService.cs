using Plag.Backend.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plag.Backend.Services
{
    /// <summary>
    /// The service for storing plagiarism reports.
    /// </summary>
    public interface IStoreService
    {
        /// <summary>
        /// Create a plagiarism set.
        /// </summary>
        /// <param name="name">The name of plagiarism set.</param>
        /// <returns>The created entity.</returns>
        Task<PlagiarismSet> CreateSetAsync(string name);

        /// <summary>
        /// Find a plagiarism set.
        /// </summary>
        /// <param name="id">The id of plagiarism set.</param>
        /// <returns>The found entity.</returns>
        Task<PlagiarismSet> FindSetAsync(string id);

        /// <summary>
        /// Find the plagiarism report.
        /// </summary>
        /// <param name="id">The id of plagiarism report.</param>
        /// <returns>The found entity.</returns>
        Task<Report> FindReportAsync(string id);

        /// <summary>
        /// Check whether a language exists.
        /// </summary>
        /// <param name="langName">The name of language.</param>
        /// <returns>The existence of language.</returns>
        Task<LanguageInfo> FindLanguageAsync(string langName);

        /// <summary>
        /// List existing languages.
        /// </summary>
        /// <returns>The existing languages.</returns>
        Task<List<LanguageInfo>> ListLanguageAsync();

        /// <summary>
        /// Submit a solution file.
        /// </summary>
        /// <param name="submission">The solution file to detect.</param>
        Task SubmitAsync(Submission submission);

        /// <summary>
        /// Find the submission with its files.
        /// </summary>
        /// <param name="id">The id of submission.</param>
        /// <returns>The found entity.</returns>
        Task<Submission> FindSubmissionAsync(string id, bool includeFiles = true);

        /// <summary>
        /// List the submissions in one plagiarism set.
        /// </summary>
        /// <param name="setId">The plagiarism set ID.</param>
        /// <returns>The submissions.</returns>
        Task<List<Submission>> ListSubmissionsAsync(string setId);

        /// <summary>
        /// List the plagiarism sets.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns>The sets.</returns>
        Task<PagedViewList<PlagiarismSet>> ListSetsAsync(int page);

        /// <summary>
        /// Get the compilation for such submission.
        /// </summary>
        /// <param name="submitId">The submission ID.</param>
        /// <returns>The compilation result.</returns>
        Task<Compilation> GetCompilationAsync(string submitId);
    }
}
