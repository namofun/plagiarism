using Plag.Backend.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plag.Backend.Services
{
    public abstract class PdsServiceBase<TKey> : PdsStoreServiceBase<TKey>, IJobContext
        where TKey : IEquatable<TKey>
    {
        /// <inheritdoc cref="IJobContext.CompileAsync(string, int, string, byte[])" />
        public abstract Task CompileAsync(TKey setid, int submitId, string error, byte[] result);

        /// <inheritdoc cref="IJobContext.CompileAsync(List{KeyValuePair{ValueTuple{string, int}, Compilation}})"/>
        public abstract Task CompileAsync(List<KeyValuePair<(TKey setId, int submitId), Compilation>> compilationResults);

        /// <inheritdoc cref="IJobContext.DequeueReportAsync" />
        public abstract Task<ReportTask> DequeueReportAsync();

        /// <inheritdoc cref="IJobContext.DequeueReportsBatchAsync(int)" />
        public abstract Task<List<ReportTask>> DequeueReportsBatchAsync(int batchSize = 100);

        /// <inheritdoc cref="IJobContext.DequeueSubmissionAsync" />
        public abstract Task<Submission> DequeueSubmissionAsync();

        /// <inheritdoc cref="IJobContext.DequeueSubmissionsBatchAsync(int)" />
        public abstract Task<List<Submission>> DequeueSubmissionsBatchAsync(int batchSize = 10);

        /// <inheritdoc cref="IJobContext.SaveReportAsync(ReportTask, ReportFragment)" />
        public abstract Task SaveReportAsync(ReportTask<TKey> task, ReportFragment fragment);

        /// <inheritdoc cref="IJobContext.SaveReportsAsync(List{KeyValuePair{ReportTask, ReportFragment}})" />
        public abstract Task SaveReportsAsync(List<KeyValuePair<ReportTask<TKey>, ReportFragment>> reports);

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

        Task IJobContext.CompileAsync(List<KeyValuePair<(string setId, int submitId), Compilation>> compilationResults)
        {
            List<KeyValuePair<(TKey setId, int submitId), Compilation>> converged = new();
            foreach (((string setId, int submitId) key, Compilation value) in compilationResults)
            {
                if (!TryGetKey(key.setId, out var setid))
                {
                    throw new Exception("The ID of plagiarism set is not correct.");
                }

                converged.Add(new((setid, key.submitId), value));
            }

            return CompileAsync(converged);
        }

        Task IJobContext.SaveReportAsync(ReportTask task, ReportFragment fragment)
        {
            if (task is not ReportTask<TKey> reportTask)
            {
                if (!TryGetKey(task.SetId, out var setid))
                {
                    throw new Exception("The ID of plagiarism set is not correct.");
                }

                if (!TryGetKey(task.Id, out var extid))
                {
                    throw new Exception("The ID is not correct.");
                }

                reportTask = ReportTask<TKey>.Of(extid, setid, task.SubmissionA, task.SubmissionB);
            }

            return SaveReportAsync(reportTask, fragment);
        }

        Task IJobContext.SaveReportsAsync(List<KeyValuePair<ReportTask, ReportFragment>> reports)
        {
            List<KeyValuePair<ReportTask<TKey>, ReportFragment>> reports2 = new();
            foreach ((ReportTask task, ReportFragment frag) in reports)
            {
                if (task is not ReportTask<TKey> reportTask)
                {
                    if (!TryGetKey(task.SetId, out var setid))
                    {
                        throw new Exception("The ID of plagiarism set is not correct.");
                    }

                    if (!TryGetKey(task.Id, out var extid))
                    {
                        throw new Exception("The ID is not correct.");
                    }

                    reportTask = ReportTask<TKey>.Of(extid, setid, task.SubmissionA, task.SubmissionB);
                }

                reports2.Add(new(reportTask, frag));
            }

            return SaveReportsAsync(reports2);
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
