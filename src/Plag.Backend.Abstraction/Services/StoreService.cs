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
        /// <param name="metadata">The metadata of plagiarism set.</param>
        /// <returns>The created entity.</returns>
        Task<PlagiarismSet> CreateSetAsync(SetCreation metadata);

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
        /// Gets the submission files.
        /// </summary>
        /// <param name="setId">The set ID.</param>
        /// <param name="submitId">The submission ID.</param>
        /// <returns>The submission files.</returns>
        Task<IReadOnlyList<SubmissionFile>> GetFilesAsync(string setId, int submitId);

        /// <summary>
        /// Submits a solution file.
        /// </summary>
        /// <param name="submission">The solution file to detect.</param>
        /// <returns>The created entity.</returns>
        Task<Submission> SubmitAsync(SubmissionCreation submission);

        /// <summary>
        /// Finds the submission with its files.
        /// </summary>
        /// <param name="setid">The id of plagiarism set.</param>
        /// <param name="submitid">The id of submission.</param>
        /// <param name="includeFiles">Whether to include files.</param>
        /// <returns>The found entity.</returns>
        Task<Submission> FindSubmissionAsync(string setid, int submitid, bool includeFiles = true);

        /// <summary>
        /// Lists the submissions in one plagiarism set.
        /// </summary>
        /// <param name="setid">The plagiarism set ID.</param>
        /// <param name="exclusive_category">The exclusive category ID, null for not filtered.</param>
        /// <param name="inclusive_category">The non-exclusive category ID, null for not filtered.</param>
        /// <param name="min_percent">The minimal percent to show, null for not filtered.</param>
        /// <returns>The submissions.</returns>
        Task<IReadOnlyList<Submission>> ListSubmissionsAsync(string setid, int? exclusive_category = null, int? inclusive_category = null, double? min_percent = null);

        /// <summary>
        /// Lists the plagiarism sets.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="uid">The user ID.</param>
        /// <param name="skip">The count to skip.</param>
        /// <param name="limit">The count to take.</param>
        /// <returns>The sets.</returns>
        Task<IReadOnlyList<PlagiarismSet>> ListSetsAsync(int? cid = null, int? uid = null, int? skip = null, int? limit = null);

        /// <summary>
        /// Gets the compilation for such submission.
        /// </summary>
        /// <param name="setid">The id of plagiarism set.</param>
        /// <param name="submitid">The id of submission.</param>
        /// <returns>The compilation result.</returns>
        Task<Compilation> GetCompilationAsync(string setid, int submitid);

        /// <summary>
        /// Gets the comparison between the submission and other submissions.
        /// </summary>
        /// <param name="setid">The id of plagiarism set.</param>
        /// <param name="submitid">The id of submission.</param>
        /// <returns>The comparison result.</returns>
        Task<IReadOnlyList<Comparison>> GetComparisonsBySubmissionAsync(string setid, int submitid);

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
