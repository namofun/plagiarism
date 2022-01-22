using Microsoft.Extensions.Logging;
using Plag.Backend.Models;
using Plag.Backend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plag.Backend.Jobs
{
    /// <summary>
    /// The report generating core logic.
    /// </summary>
    public class ReportGenerator
    {
        private readonly ICompileService _compiler;
        private readonly IConvertService2 _converter;
        private readonly IReportService _reporter;
        private ILogger _logger;

        public ReportGenerator(ICompileService compiler, IConvertService2 converter, IReportService reporter)
        {
            _compiler = compiler;
            _converter = converter;
            _reporter = reporter;
        }

        public void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        private async Task<(Submission, Frontend.Submission)> Load((string, int) key, IJobContext store)
        {
            var (setid, submitid) = key;
            var s = await store.FindSubmissionAsync(setid, submitid, false);
            if (s == null || s.TokenProduced != true)
            {
                throw new InvalidOperationException("Data synchronization error.");
            }

            var c = await store.GetCompilationAsync(setid, submitid);
            Frontend.Submission fe = await GetFrontendSubmissionAsync(s, c, store);
            return (s, fe);
        }

        private async Task<Frontend.Submission> GetFrontendSubmissionAsync(Submission s, Compilation c, IJobContext store)
        {
            var lang = _compiler.FindLanguage(s.Language);

            if (c?.Tokens == null)
            {
                _logger?.LogWarning("Token for {SetId}\\s{SubmissionId} missing, generating at once.", s.SetId, s.Id);
                var fs = s.Files ?? await store.GetFilesAsync(s.SetId, s.Id);
                return new Frontend.Submission(lang, new Frontend.SubmissionFileProxy(fs), s.ExternalId);
            }
            else
            {
                var tokens = _converter.TokenDeserialize(c.Tokens, lang);
                return new Frontend.Submission(lang, null, s.ExternalId, tokens);
            }
        }

        private async Task<Dictionary<(string, int), (Submission, Frontend.Submission)>> LoadBatch(List<(string, int)> keys, IJobContext store)
        {
            var entities = await store.GetSubmissionsAsync(keys);
            var result = new Dictionary<(string, int), (Submission, Frontend.Submission)>();
            foreach (var (s, c) in entities)
            {
                Frontend.Submission fe = await GetFrontendSubmissionAsync(s, c, store);
                result.Add((s.SetId, s.Id), (s, fe));
            }

            return result;
        }

        private ReportFragment MatchReportCreate(bool swapAB, Frontend.Matching matching)
        {
            return new ReportFragment
            {
                TokensMatched = matching.TokensMatched,
                BiggestMatch = matching.BiggestMatch,
                Percent = matching.Percent,
                Matches = _converter.MatchSerialize(matching, swapAB),
                PercentA = swapAB ? matching.PercentB : matching.PercentA,
                PercentB = swapAB ? matching.PercentA : matching.PercentB,
            };
        }

        /// <summary>
        /// Chooses one report from queue and generate.
        /// </summary>
        /// <param name="context">The database context for job scheduling.</param>
        /// <param name="lru">The LRU store.</param>
        /// <returns>Whether any report is proceeded.</returns>
        public async Task<bool> DoWorkAsync(IJobContext context, LruStore<(string, int), (Submission, Frontend.Submission)> lru)
        {
            var rep = await context.DequeueReportAsync();
            if (rep == null) return false;

            var (ss0, ss0t) = await lru.GetOrLoadAsync((rep.SetId, rep.SubmissionA), context, Load);
            var (ss1, ss1t) = await lru.GetOrLoadAsync((rep.SetId, rep.SubmissionB), context, Load);

            var result = _reporter.Generate(ss0t, ss1t);
            var frag = MatchReportCreate(ss0.ExternalId != result.SubmissionA.Id, result);
            await context.SaveReportAsync(rep, frag);
            return true;
        }

        /// <summary>
        /// Chooses one report from queue and generate.
        /// </summary>
        /// <param name="context">The database context for job scheduling.</param>
        /// <param name="lru">The LRU store.</param>
        /// <returns>Count of reports proceeded.</returns>
        public async Task<int> DoWorkBatchAsync(IJobContext context, LruStore<(string, int), (Submission, Frontend.Submission)> lru)
        {
            var tasks = await context.DequeueReportsBatchAsync();
            if (tasks.Count == 0) return 0;

            await lru.LoadBatchAsync(
                tasks.Select(r => (r.SetId, r.SubmissionA))
                    .Concat(tasks.Select(r => (r.SetId, r.SubmissionB)))
                    .Distinct()
                    .ToList(),
                k => LoadBatch(k, context));

            List<KeyValuePair<ReportTask, ReportFragment>> batch = new();
            foreach (var rep in tasks)
            {
                var (ss0, ss0t) = await lru.GetOrLoadAsync((rep.SetId, rep.SubmissionA), context, Load);
                var (ss1, ss1t) = await lru.GetOrLoadAsync((rep.SetId, rep.SubmissionB), context, Load);

                var result = _reporter.Generate(ss0t, ss1t);
                var frag = MatchReportCreate(ss0.ExternalId != result.SubmissionA.Id, result);
                batch.Add(new(rep, frag));
            }

            await context.SaveReportsAsync(batch);
            return batch.Count;
        }
    }
}
