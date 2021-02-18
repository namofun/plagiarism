using Plag.Backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plag.Backend.Services
{
    /// <summary>
    /// The service for storing plagiarism reports.
    /// </summary>
    public interface IPlagiarismDetectService
    {
        /// <summary>
        /// Creates a plagiarism set.
        /// </summary>
        /// <param name="name">The name of plagiarism set.</param>
        /// <returns>The created entity.</returns>
        Task<PlagiarismSet> CreateSetAsync(string name);

        /// <summary>
        /// Finds a plagiarism set.
        /// </summary>
        /// <param name="id">The id of plagiarism set.</param>
        /// <returns>The found entity.</returns>
        Task<PlagiarismSet> FindSetAsync(string id);

        /// <summary>
        /// Finds the plagiarism report.
        /// </summary>
        /// <param name="id">The id of plagiarism report.</param>
        /// <returns>The found entity.</returns>
        Task<Report> FindReportAsync(string id);

        /// <summary>
        /// Checks whether a language exists.
        /// </summary>
        /// <param name="langName">The name of language.</param>
        /// <returns>The existence of language.</returns>
        Task<LanguageInfo> FindLanguageAsync(string langName);

        /// <summary>
        /// Lists existing languages.
        /// </summary>
        /// <returns>The existing languages.</returns>
        Task<List<LanguageInfo>> ListLanguageAsync();

        /// <summary>
        /// Submits a solution file.
        /// </summary>
        /// <param name="submission">The solution file to detect.</param>
        Task<Submission> SubmitAsync(SubmissionCreation submission);

        /// <summary>
        /// Finds the submission with its files.
        /// </summary>
        /// <param name="id">The id of submission.</param>
        /// <param name="includeFiles">Whether to include files.</param>
        /// <returns>The found entity.</returns>
        Task<Submission> FindSubmissionAsync(string id, bool includeFiles = true);

        /// <summary>
        /// Lists the submissions in one plagiarism set.
        /// </summary>
        /// <param name="setId">The plagiarism set ID.</param>
        /// <returns>The submissions.</returns>
        Task<List<Submission>> ListSubmissionsAsync(string setId);

        /// <summary>
        /// Lists the plagiarism sets.
        /// </summary>
        /// <param name="skip">The count to skip.</param>
        /// <param name="limit">The count to take.</param>
        /// <returns>The sets.</returns>
        Task<List<PlagiarismSet>> ListSetsAsync(int? skip = null, int? limit = null);

        /// <summary>
        /// Gets the compilation for such submission.
        /// </summary>
        /// <param name="submitId">The submission ID.</param>
        /// <returns>The compilation result.</returns>
        Task<Compilation> GetCompilationAsync(string submitId);

        /// <summary>
        /// Gets the comparison between the submission and other submissions.
        /// </summary>
        /// <param name="submitId">The submission ID.</param>
        /// <returns>The comparison result.</returns>
        Task<List<Comparison>> GetComparisonsBySubmissionAsync(string submitId);

        /// <summary>
        /// Sends a signal and try to rescue the background service.
        /// </summary>
        /// <returns>The rescue signal task.</returns>
        Task RescueAsync();

        /// <summary>
        /// Gets the version object.
        /// </summary>
        /// <returns>The version object.</returns>
        object GetVersion();
    }
}
