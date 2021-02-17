using Microsoft.Extensions.Caching.Memory;
using Plag.Backend.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plag.Backend.Services
{
    public interface IStoreExtService : IPlagiarismDetectService
    {
        /// <summary>
        /// Set the submission compiled with error and result.
        /// </summary>
        /// <param name="submit">The submission.</param>
        /// <param name="error">The compile error.</param>
        /// <param name="result">The compiled tokens.</param>
        Task CompileAsync(Submission submit, string error, byte[] result);

        /// <summary>
        /// Fetch one pending submission.
        /// </summary>
        /// <returns>The pending submission.</returns>
        Task<Submission> FetchAsync();

        /// <summary>
        /// Schedule the submission with other submissions.
        /// </summary>
        /// <param name="submit">The submission.</param>
        Task ScheduleAsync(Submission submit);

        /// <summary>
        /// Find the submission with local cache.
        /// </summary>
        /// <param name="submit">The submission ID.</param>
        /// <returns>The submission.</returns>
        ValueTask<Submission> QuickFindSubmissionAsync(string submit);

        /// <summary>
        /// Get the files.
        /// </summary>
        /// <param name="submitId">The submission ID.</param>
        /// <returns>The submission files.</returns>
        Task<List<SubmissionFile>> GetFilesAsync(string submitId);

        /// <summary>
        /// Fetch one pending report.
        /// </summary>
        /// <returns>The report.</returns>
        Task<Report> FetchOneAsync();

        /// <summary>
        /// Save the report.
        /// </summary>
        /// <param name="setId">The set ID.</param>
        /// <param name="report">The report.</param>
        Task SaveReportAsync(string setId, Report report);

        /// <summary>
        /// Transient store in this scope
        /// </summary>
        IMemoryCache TransientStore { get; }
    }
}
