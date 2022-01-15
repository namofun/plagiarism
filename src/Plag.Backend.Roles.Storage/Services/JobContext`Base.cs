using Plag.Backend.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plag.Backend.Services
{
    public abstract class PdsServiceBase<TKey> : PdsStoreServiceBase<TKey>, IJobContext
        where TKey : IEquatable<TKey>
    {
        /// <inheritdoc cref="IJobContext.MigrateAsync" />
        public abstract Task MigrateAsync();

        /// <inheritdoc cref="IJobContext.UpdateLanguagesAsync(List{LanguageInfo})" />
        public abstract Task UpdateLanguagesAsync(List<LanguageInfo> languageSeeds);

        /// <inheritdoc cref="IJobContext.RefreshCacheAsync" />
        public abstract Task RefreshCacheAsync();

        /// <inheritdoc cref="IJobContext.CompileAsync(Submission, string, byte[])" />
        public abstract Task CompileAsync(TKey setid, int submitId, Submission submission, string error, byte[] result);

        /// <inheritdoc cref="IJobContext.DequeueReportAsync" />
        public abstract Task<ReportTask> DequeueReportAsync();

        /// <inheritdoc cref="IJobContext.DequeueReportsBatchAsync(int)" />
        public abstract Task<List<ReportTask>> DequeueReportsBatchAsync(int batchSize = 20);

        /// <inheritdoc cref="IJobContext.DequeueSubmissionAsync" />
        public abstract Task<Submission> DequeueSubmissionAsync();

        /// <inheritdoc cref="IJobContext.SaveReportAsync(ReportTask, ReportFragment)" />
        public abstract Task SaveReportAsync(ReportTask<TKey> task, ReportFragment fragment);

        /// <inheritdoc cref="IJobContext.SaveReportsAsync(List{KeyValuePair{ReportTask, ReportFragment}})" />
        public abstract Task SaveReportsAsync(List<KeyValuePair<ReportTask<TKey>, ReportFragment>> reports);

        /// <inheritdoc cref="IJobContext.ScheduleAsync(Submission)" />
        public abstract Task<int> ScheduleAsync(TKey setId, int submitId, int exclusive, int inclusive, string langId);

        /// <inheritdoc cref="IJobContext.GetSubmissionsAsync(List{string})" />
        public abstract Task<List<KeyValuePair<Submission, Compilation>?>> GetSubmissionsAsync(List<TKey> submitExternalIds);

        /// <inheritdoc cref="IJobContext.GetSubmissionsAsync(List{ValueTuple{string,int}})" />
        public abstract Task<List<KeyValuePair<Submission, Compilation>>> GetSubmissionsAsync(List<(TKey, int)> submitIds);

        Task IJobContext.CompileAsync(Submission submission, string error, byte[] result)
        {
            if (!TryGetKey(submission.SetId, out var setid))
            {
                throw new Exception("The ID of plagiarism set is not correct.");
            }

            return CompileAsync(setid, submission.Id, submission, error, result);
        }

        Task<List<KeyValuePair<Submission, Compilation>?>> IJobContext.GetSubmissionsAsync(List<string> submitExternalIds)
        {
            List<TKey> extids = new();
            foreach (var id in submitExternalIds)
            {
                if (!TryGetKey(id, out var guid))
                {
                    throw new Exception("The ID of submission is not correct.");
                }

                extids.Add(guid);
            }

            return GetSubmissionsAsync(extids);
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

        Task<int> IJobContext.ScheduleAsync(Submission s)
        {
            if (!TryGetKey(s.SetId, out var setid))
            {
                throw new Exception("The ID of plagiarism set is not correct.");
            }

            return ScheduleAsync(setid, s.Id, s.ExclusiveCategory, s.InclusiveCategory, s.Language);
        }

        Task<List<KeyValuePair<Submission, Compilation>>> IJobContext.GetSubmissionsAsync(List<(string, int)> submitIds)
        {
            List<(TKey, int)> submitIds2 = new();
            foreach ((string setId, int subId) in submitIds)
            {
                if (!TryGetKey(setId, out TKey setIdd)) continue;
                submitIds2.Add((setIdd, subId));
            }

            return GetSubmissionsAsync(submitIds2);
        }
    }
}
