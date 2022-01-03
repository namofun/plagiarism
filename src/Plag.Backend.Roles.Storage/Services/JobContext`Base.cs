using Plag.Backend.Jobs;
using Plag.Backend.Models;
using System;
using System.Threading.Tasks;

namespace Plag.Backend.Services
{
    public abstract class PdsServiceBase<TKey> : PdsStoreServiceBase<TKey>, IJobContext
        where TKey : IEquatable<TKey>
    {
        /// <inheritdoc cref="IJobContext.CompileAsync(string, int, string, byte[])" />
        public abstract Task CompileAsync(TKey setid, int submitId, string error, byte[] result);

        /// <inheritdoc cref="IJobContext.DequeueReportAsync" />
        public abstract Task<ReportTask> DequeueReportAsync();

        /// <inheritdoc cref="IJobContext.DequeueSubmissionAsync" />
        public abstract Task<Submission> DequeueSubmissionAsync();

        /// <inheritdoc cref="IJobContext.SaveReportAsync(ReportTask, ReportFragment)" />
        public abstract Task SaveReportAsync(TKey setid, int a, int b, TKey extid, ReportFragment fragment);

        /// <inheritdoc cref="IJobContext.ScheduleAsync(string, int, int, int, string)" />
        public abstract Task ScheduleAsync(TKey setId, int submitId, int exclusive, int inclusive, string langId);


        Task IJobContext.CompileAsync(string setId, int submitId, string error, byte[] result)
        {
            if (!TryGetKey(setId, out var setid))
            {
                throw new Exception("The ID of plagiarism set is not correct.");
            }

            return CompileAsync(setid, submitId, error, result);
        }

        Task IJobContext.SaveReportAsync(ReportTask task, ReportFragment fragment)
        {
            if (!TryGetKey(task.SetId, out var setid))
            {
                throw new Exception("The ID of plagiarism set is not correct.");
            }

            if (!TryGetKey(task.Id, out var extid))
            {
                throw new Exception("The ID is not correct.");
            }

            return SaveReportAsync(setid, task.SubmissionA, task.SubmissionB, extid, fragment);
        }

        Task IJobContext.ScheduleAsync(string setId, int submitId, int exclusive, int inclusive, string langId)
        {
            if (!TryGetKey(setId, out var setid))
            {
                throw new Exception("The ID of plagiarism set is not correct.");
            }

            return ScheduleAsync(setid, submitId, exclusive, inclusive, langId);
        }
    }
}
