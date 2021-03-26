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
        public abstract ServiceVersion GetVersion();

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

        /// <inheritdoc cref="IPlagiarismDetectService.JustificateAsync(string, bool?)" />
        public abstract Task JustificateAsync(TKey reportid, bool? status);

        /// <inheritdoc cref="IPlagiarismDetectService.ListSetsAsync(int?, int?, int?, int?, bool)" />
        public abstract Task<List<PlagiarismSet<TKey>>> ListSetsAsync(int? cid = null, int? uid = null, int? skip = null, int? limit = null, bool asc = false);

        /// <inheritdoc cref="IPlagiarismDetectService.ListSubmissionsAsync(string, string, int?, int?, double?, int?, int?, string, bool)" />
        public abstract Task<List<Submission<TKey>>> ListSubmissionsAsync(TKey setid, string language = null, int? exclusive_category = null, int? inclusive_category = null, double? min_percent = null, int? skip = null, int? limit = null, string order = "id", bool asc = true);

        /// <inheritdoc cref="IPlagiarismDetectService.SubmitAsync(SubmissionCreation)" />
        public abstract Task<Submission<TKey>> SubmitAsync(TKey setId, SubmissionCreation submission);

        /// <inheritdoc cref="IPlagiarismDetectService.GetFilesAsync(string, int)" />
        /// <param name="extId">The external submission ID.</param>
        public abstract Task<IReadOnlyList<SubmissionFile>> GetFilesAsync(TKey extId);


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
            var files = await GetFilesAsync(entity.ExternalId);
            return entity.ToModel(files);
        }

        async Task<IReadOnlyList<Submission>> IPlagiarismDetectService.ListSubmissionsAsync(string _setId, string language, int? exclusive_category, int? inclusive_category, double? min_percent, int? skip, int? limit, string order, bool asc)
        {
            if (!TryGetKey(_setId, out var setid)) return null;
            var result = await ListSubmissionsAsync(setid, language, exclusive_category, inclusive_category, min_percent, skip, limit, order, asc);
            return result.Select(r => r.ToModel()).ToList();
        }

        async Task<IReadOnlyList<PlagiarismSet>> IPlagiarismDetectService.ListSetsAsync(int? cid, int? uid, int? skip, int? limit, bool asc)
        {
            var result = await ListSetsAsync(cid, uid, skip, limit, asc);
            return result.Select(r => r.ToModel()).ToList();
        }

        public async Task<Compilation> GetCompilationAsync(string setId, int submitid)
        {
            if (!TryGetKey(setId, out var setid)) return null;
            return await GetCompilationAsync(setid, submitid);
        }

        async Task<Vertex> IPlagiarismDetectService.GetComparisonsBySubmissionAsync(string setId, int submitid)
        {
            if (!TryGetKey(setId, out var setid)) return null;
            var submit = await FindSubmissionAsync(setid, submitid);
            if (submit == null) return null;

            var returns = submit.To<Vertex>();
            returns.Comparisons = await GetComparisonsBySubmissionAsync(setid, submitid);
            return returns;
        }

        Task IPlagiarismDetectService.JustificateAsync(string reportid, bool? status)
        {
            if (!TryGetKey(reportid, out var reportId))
            {
                throw new KeyNotFoundException("The report doesn't exists.");
            }

            return JustificateAsync(reportId, status);
        }

        public virtual async Task<IReadOnlyList<SubmissionFile>> GetFilesAsync(string setId, int submitId)
        {
            if (!TryGetKey(setId, out var setid)) return null;
            var submit = await FindSubmissionAsync(setid, submitId);
            if (submit == null) return null;
            return await GetFilesAsync(submit.ExternalId);
        }

        #endregion
    }
}
