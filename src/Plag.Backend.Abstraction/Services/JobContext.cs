using System.Collections.Generic;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Models;

namespace Xylab.PlagiarismDetect.Backend.Services
{
    /// <summary>
    /// The contract for finishing jobs.
    /// </summary>
    public interface IJobContext
    {
        /// <summary>
        /// Gets a value indicating whether service graph is supported.
        /// </summary>
        bool SupportServiceGraph { get; }

        /// <summary>
        /// Migrates the database schema.
        /// </summary>
        Task MigrateAsync();

        /// <summary>
        /// Updates the languages list.
        /// </summary>
        /// <param name="languageSeeds">The languages list.</param>
        Task UpdateLanguagesAsync(List<LanguageInfo> languageSeeds);

        /// <summary>
        /// Refreshes the cache fields.
        /// </summary>
        Task RefreshCacheAsync();

        /// <summary>
        /// Sets the submission compiled with error and result.
        /// </summary>
        /// <param name="submission">The submission entity snapshot.</param>
        /// <param name="error">The compile error.</param>
        /// <param name="result">The compiled tokens.</param>
        Task CompileAsync(Submission submission, string error, byte[] result);

        /// <summary>
        /// Dequeues one pending submission.
        /// </summary>
        /// <returns>The pending submission.</returns>
        Task<Submission> DequeueSubmissionAsync();

        /// <summary>
        /// Dequeues one pending report.
        /// </summary>
        /// <returns>The pending report.</returns>
        Task<ReportTask> DequeueReportAsync();

        /// <summary>
        /// Dequeues one batch of at least <paramref name="batchSize"/> pending reports.
        /// </summary>
        /// <returns>The pending reports.</returns>
        Task<List<ReportTask>> DequeueReportsBatchAsync(int batchSize = 20);

        /// <summary>
        /// Schedules the report of this submission with other submissions.
        /// </summary>
        /// <param name="submission">The submission.</param>
        /// <remarks>This will not send a signal for report generation service to work.</remarks>
        Task<int> ScheduleAsync(Submission submission);

        /// <summary>
        /// Gets the compilation for such submission.
        /// </summary>
        /// <param name="setId">The set ID.</param>
        /// <param name="submitId">The submission ID.</param>
        /// <returns>The compilation result.</returns>
        Task<Compilation> GetCompilationAsync(string setId, int submitId);

        /// <summary>
        /// Saves the report.
        /// </summary>
        /// <param name="task">The report task.</param>
        /// <param name="fragment">The report fragment.</param>
        Task SaveReportAsync(ReportTask task, ReportFragment fragment);

        /// <summary>
        /// Saves the reports.
        /// </summary>
        /// <param name="reports">The report tasks and fragments.</param>
        Task SaveReportsAsync(List<KeyValuePair<ReportTask, ReportFragment>> reports);

        /// <summary>
        /// Finds the submission with its files.
        /// </summary>
        /// <param name="setId">The set ID.</param>
        /// <param name="submitId">The submission ID.</param>
        /// <param name="includeFiles">Whether to include files.</param>
        /// <returns>The found entity.</returns>
        Task<Submission> FindSubmissionAsync(string setId, int submitId, bool includeFiles = true);

        /// <summary>
        /// Gets the submission files.
        /// </summary>
        /// <param name="setId">The set ID.</param>
        /// <param name="submitId">The submission ID.</param>
        /// <returns>The submission files.</returns>
        Task<IReadOnlyList<SubmissionFile>> GetFilesAsync(string setId, int submitId);

        /// <summary>
        /// Gets several submissions with its compilation in one batch.
        /// </summary>
        /// <param name="submitExternalIds">The list of submission external IDs.</param>
        /// <returns>The submission with files.</returns>
        Task<List<KeyValuePair<Submission, Compilation>?>> GetSubmissionsAsync(List<string> submitExternalIds);

        /// <summary>
        /// Gets several submissions with its compilation in one batch.
        /// </summary>
        /// <param name="submitIds">The list of submission internal IDs.</param>
        /// <returns>The submission with files.</returns>
        Task<List<KeyValuePair<Submission, Compilation>>> GetSubmissionsAsync(List<(string, int)> submitIds);
    }

    /// <summary>
    /// The contract for service graph.
    /// </summary>
    public interface IServiceGraphContext
    {
        /// <summary>
        /// Finds a plagiarism set.
        /// </summary>
        /// <param name="id">The id of plagiarism set.</param>
        /// <returns>The found entity.</returns>
        Task<PlagiarismSet> FindSetAsync(string id);

        /// <summary>
        /// Gets the vertices from service graph.
        /// </summary>
        /// <param name="setId">The set ID.</param>
        /// <returns>The list of vertices.</returns>
        Task<List<ServiceVertex>> GetVerticesAsync(string setId);

        /// <summary>
        /// Gets the edges from service graph.
        /// </summary>
        /// <param name="setId">The set ID.</param>
        /// <returns>The list of edges.</returns>
        Task<List<ServiceEdge>> GetEdgesAsync(string setId);
    }
}
