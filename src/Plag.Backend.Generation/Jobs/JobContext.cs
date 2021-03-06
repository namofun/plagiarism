﻿using Plag.Backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plag.Backend.Jobs
{
    /// <summary>
    /// The contract for finishing jobs.
    /// </summary>
    public interface IJobContext
    {
        /// <summary>
        /// Sets the submission compiled with error and result.
        /// </summary>
        /// <param name="setid">The set ID.</param>
        /// <param name="submitId">The submission ID.</param>
        /// <param name="error">The compile error.</param>
        /// <param name="result">The compiled tokens.</param>
        Task CompileAsync(string setid, int submitId, string error, byte[] result);

        /// <summary>
        /// Dequeues one pending submission.
        /// </summary>
        /// <returns>The pending submission.</returns>
        Task<Submission> DequeueSubmissionAsync();

        /// <summary>
        /// Dequeues one pending report.
        /// </summary>
        /// <returns>The report.</returns>
        Task<ReportTask> DequeueReportAsync();

        /// <summary>
        /// Schedules the report of this submission with other submissions.
        /// </summary>
        /// <remarks>This will not send a signal for report generation service to work.</remarks>
        /// <param name="langId">The language ID.</param>
        /// <param name="setId">The set ID.</param>
        /// <param name="exclusive">The exclusive category.</param>
        /// <param name="inclusive">The inclusive category.</param>
        /// <param name="submitId">The submission ID.</param>
        Task ScheduleAsync(string setId, int submitId, int exclusive, int inclusive, string langId);

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
        /// <param name="task">The repor task.</param>
        /// <param name="fragment">The report fragment.</param>
        Task SaveReportAsync(ReportTask task, ReportFragment fragment);

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
    }
}
