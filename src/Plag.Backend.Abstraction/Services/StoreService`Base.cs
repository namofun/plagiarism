using Plag.Backend.Entities;
using Plag.Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plag.Backend.Services
{
    public abstract class PdsStoreServiceBase<TKey> : IPlagiarismDetectService
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Try to parse the <paramref name="id"/> to be <typeparamref name="TKey"/>.
        /// </summary>
        /// <param name="id">The id to parse.</param>
        /// <param name="key">The parsed key.</param>
        /// <returns>Whether parse succeeded.</returns>
        public abstract bool TryGetKey(string id, out TKey key);

        /// <inheritdoc />
        public abstract object GetVersion();

        /// <inheritdoc />
        public abstract Task<List<LanguageInfo>> ListLanguageAsync();

        /// <inheritdoc />
        public abstract Task<LanguageInfo> FindLanguageAsync(string langName);

        /// <inheritdoc />
        public abstract Task RescueAsync();

        /// <inheritdoc cref="IPlagiarismDetectService.CreateSetAsync(SetCreation)" />
        public abstract Task<PlagiarismSet<TKey>> CreateSetAsync(SetCreation metadata);

        /// <inheritdoc cref="IPlagiarismDetectService.FindReportAsync(string)" />
        public abstract Task<Report<TKey>> FindReportAsync(TKey id);

        /// <inheritdoc cref="IPlagiarismDetectService.FindSetAsync(string)" />
        public abstract Task<PlagiarismSet<TKey>> FindSetAsync(TKey id);

        /// <inheritdoc cref="IPlagiarismDetectService.FindSubmissionAsync(string, int, bool)" />
        public abstract Task<Submission<TKey>> FindSubmissionAsync(TKey setid, int submitid);

        /// <inheritdoc cref="IPlagiarismDetectService.GetComparisonsBySubmissionAsync(string, int)" />
        public abstract Task<IReadOnlyList<Comparison>> GetComparisonsBySubmissionAsync(TKey setid, int submitid);

        /// <inheritdoc cref="IPlagiarismDetectService.GetCompilationAsync(string, int)" />
        public abstract Task<Compilation> GetCompilationAsync(TKey setid, int submitid);

        /// <inheritdoc cref="IPlagiarismDetectService.ListSetsAsync(int?, int?, int?, int?)" />
        public abstract Task<List<PlagiarismSet<TKey>>> ListSetsAsync(int? cid = null, int? uid = null, int? skip = null, int? limit = null);

        /// <inheritdoc cref="IPlagiarismDetectService.ListSubmissionsAsync(string, int?, int?, double?)" />
        public abstract Task<List<Submission<TKey>>> ListSubmissionsAsync(TKey setid, int? exclusive_category, int? inclusive_category, double? min_percent);

        /// <inheritdoc cref="IPlagiarismDetectService.SubmitAsync(SubmissionCreation)" />
        public abstract Task<Submission<TKey>> SubmitAsync(TKey setId, SubmissionCreation submission);

        /// <inheritdoc cref="IPlagiarismDetectService.GetFilesAsync(string, int)" />
        public abstract Task<IReadOnlyList<SubmissionFile>> GetFilesAsync(TKey setId, int submitId);


        #region Explicit Implementations

        async Task<PlagiarismSet> IPlagiarismDetectService.CreateSetAsync(SetCreation metadata)
        {
            var entity = await CreateSetAsync(metadata);
            return entity.ToModel();
        }

        async Task<PlagiarismSet> IPlagiarismDetectService.FindSetAsync(string _id)
        {
            if (!TryGetKey(_id, out var id)) return null;
            var entity = await FindSetAsync(id);
            return entity?.ToModel();
        }

        async Task<Report> IPlagiarismDetectService.FindReportAsync(string _id)
        {
            if (!TryGetKey(_id, out var id)) return null;
            var entity = await FindReportAsync(id);
            return entity?.ToModel();
        }

        async Task<Submission> IPlagiarismDetectService.SubmitAsync(SubmissionCreation submission)
        {
            if (!TryGetKey(submission.SetId, out var setid))
            {
                throw new ArgumentOutOfRangeException("SetId", "Set not found.");
            }

            var entity = await SubmitAsync(setid, submission);
            return entity.ToModel();
        }

        public async Task<Submission> FindSubmissionAsync(string setId, int submitid, bool includeFiles)
        {
            if (!TryGetKey(setId, out var setid)) return null;
            var entity = await FindSubmissionAsync(setid, submitid);
            if (entity == null || !includeFiles) return entity?.ToModel();
            var files = await GetFilesAsync(entity.SetId, entity.Id);
            return entity.ToModel(files);
        }

        async Task<IReadOnlyList<Submission>> IPlagiarismDetectService.ListSubmissionsAsync(string _setId, int? exclusive_category, int? inclusive_category, double? min_percent)
        {
            if (!TryGetKey(_setId, out var setid)) return null;
            var result = await ListSubmissionsAsync(setid, exclusive_category, inclusive_category, min_percent);
            return result.Select(r => r.ToModel()).ToList();
        }

        async Task<IReadOnlyList<PlagiarismSet>> IPlagiarismDetectService.ListSetsAsync(int? cid, int? uid, int? skip, int? limit)
        {
            var result = await ListSetsAsync(cid, uid, skip, limit);
            return result.Select(r => r.ToModel()).ToList();
        }

        public async Task<Compilation> GetCompilationAsync(string setId, int submitid)
        {
            if (!TryGetKey(setId, out var setid)) return null;
            return await GetCompilationAsync(setid, submitid);
        }

        Task<IReadOnlyList<Comparison>> IPlagiarismDetectService.GetComparisonsBySubmissionAsync(string setId, int submitid)
        {
            if (!TryGetKey(setId, out var setid)) return Task.FromResult<IReadOnlyList<Comparison>>(Array.Empty<Comparison>());
            return GetComparisonsBySubmissionAsync(setid, submitid);
        }

        public Task<IReadOnlyList<SubmissionFile>> GetFilesAsync(string setId, int submitId)
        {
            if (!TryGetKey(setId, out var setid)) return Task.FromResult<IReadOnlyList<SubmissionFile>>(Array.Empty<SubmissionFile>());
            return GetFilesAsync(setid, submitId);
        }

        #endregion
    }
}
