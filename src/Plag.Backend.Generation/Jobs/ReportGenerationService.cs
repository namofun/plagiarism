using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plag.Backend.Models;
using Plag.Backend.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Plag.Backend.Jobs
{
    public class ReportGenerationService : ContextNotifyService<ReportGenerationService>
    {
        public IConvertService2 Convert { get; }

        public ICompileService Compile { get; }

        public IReportService Report { get; }

        public ReportGenerationService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Convert = serviceProvider.GetRequiredService<IConvertService2>();
            Compile = serviceProvider.GetRequiredService<ICompileService>();
            Report = serviceProvider.GetRequiredService<IReportService>();
        }

        private async Task<(Submission, Frontend.Submission)> Load(string id, IJobContext store)
        {
            var s = await store.FindSubmissionAsync(id);
            if (s == null || s.TokenProduced != true)
            {
                throw new InvalidOperationException("Data synchronization error.");
            }

            var c = await store.GetCompilationAsync(id);
            var lang = Compile.FindLanguage(s.Language);
            Frontend.Submission fe;

            if (c?.Tokens == null)
            {
                Logger.LogWarning($"Token for s{s.Id} missing, generating at once.");
                var fs = await store.GetFilesAsync(s.Id);
                fe = new Frontend.Submission(lang, new Frontend.SubmissionFileProxy(fs), s.Id);
            }
            else
            {
                var tokens = Convert.TokenDeserialize(c.Tokens, lang);
                fe = new Frontend.Submission(lang, null, s.Id, tokens);
            }

            return (s, fe);
        }

        private ReportFragment MatchReportCreate(ReportTask task, Frontend.Matching matching)
        {
            bool swapAB = task.SubmissionA != matching.SubmissionA.Id;
            return new ReportFragment
            {
                TokensMatched = matching.TokensMatched,
                BiggestMatch = matching.BiggestMatch,
                Percent = matching.Percent,
                Matches = Convert.MatchSerialize(matching, swapAB),
                PercentA = swapAB ? matching.PercentB : matching.PercentA,
                PercentB = swapAB ? matching.PercentA : matching.PercentB,
            };
        }

        private async Task<bool> ResolveAsync(IJobContext context, LruStore<string, (Submission, Frontend.Submission)> lru)
        {
            var rep = await context.DequeueReportAsync();
            if (rep == null) return false;

            var (ss0, ss0t) = await lru.GetOrLoadAsync(rep.SubmissionA, context, Load);
            var (ss1, ss1t) = await lru.GetOrLoadAsync(rep.SubmissionB, context, Load);

            var result = Report.Generate(ss0t, ss1t);
            var frag = MatchReportCreate(rep, result);
            await context.SaveReportAsync(ss0.SetId, rep, frag);
            return true;
        }

        protected override async Task ProcessAsync(IJobContext context, CancellationToken stoppingToken)
        {
            var lru = new LruStore<string, (Submission, Frontend.Submission)>();
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!await ResolveAsync(context, lru)) break;
            }
        }
    }
}
