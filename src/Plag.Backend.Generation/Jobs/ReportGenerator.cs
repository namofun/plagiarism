using Microsoft.Extensions.Logging;
using Plag.Backend.Models;
using Plag.Backend.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plag.Backend.Jobs
{
    /// <summary>
    /// The report generating core logic.
    /// </summary>
    public class ReportGenerator
    {
        private readonly ILogger _logger;
        private readonly ICompileService _compiler;
        private readonly IConvertService2 _converter;
        private readonly IReportService _reporter;

        public ReportGenerator(ILogger logger, ICompileService compiler, IConvertService2 converter, IReportService reporter)
        {
            _logger = logger;
            _compiler = compiler;
            _converter = converter;
            _reporter = reporter;
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
            var lang = _compiler.FindLanguage(s.Language);
            Frontend.Submission fe;

            if (c?.Tokens == null)
            {
                _logger.LogWarning("Token for {SetId}\\s{SubmissionId} missing, generating at once.", s.SetId, s.Id);
                var fs = await store.GetFilesAsync(setid, submitid);
                fe = new Frontend.Submission(lang, new Frontend.SubmissionFileProxy(fs), s.ExternalId);
            }
            else
            {
                var tokens = _converter.TokenDeserialize(c.Tokens, lang);
                fe = new Frontend.Submission(lang, null, s.ExternalId, tokens);
            }

            return (s, fe);
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
        /// <returns>Whether any report is proceeded.</returns>
        public async Task<bool> DoWorkBatchAsync(IJobContext context, LruStore<(string, int), (Submission, Frontend.Submission)> lru)
        {
            var tasks = await context.DequeueReportsBatchAsync();
            if (tasks.Count == 0) return false;

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
            return true;
        }
    }
}
