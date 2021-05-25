using Jobs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plag.Backend.Models;
using Plag.Backend.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Plag.Backend.Jobs
{
    public class ReportGenerationService : BackgroundNotifiableService<ReportGenerationService>
    {
        public ILogger<ReportGenerationService> Logger { get; }

        public IConvertService2 Convert { get; }

        public ICompileService Compile { get; }

        public IReportService Report { get; }

        public ReportGenerationService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Convert = serviceProvider.GetRequiredService<IConvertService2>();
            Compile = serviceProvider.GetRequiredService<ICompileService>();
            Report = serviceProvider.GetRequiredService<IReportService>();
            Logger = serviceProvider.GetRequiredService<ILogger<ReportGenerationService>>();
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
            var lang = Compile.FindLanguage(s.Language);
            Frontend.Submission fe;

            if (c?.Tokens == null)
            {
                Logger.LogWarning($"Token for {s.SetId}\\s{s.Id} missing, generating at once.");
                var fs = await store.GetFilesAsync(setid, submitid);
                fe = new Frontend.Submission(lang, new Frontend.SubmissionFileProxy(fs), s.ExternalId);
            }
            else
            {
                var tokens = Convert.TokenDeserialize(c.Tokens, lang);
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
                Matches = Convert.MatchSerialize(matching, swapAB),
                PercentA = swapAB ? matching.PercentB : matching.PercentA,
                PercentB = swapAB ? matching.PercentA : matching.PercentB,
            };
        }

        private async Task<bool> ResolveAsync(IJobContext context, LruStore<(string, int), (Submission, Frontend.Submission)> lru)
        {
            var rep = await context.DequeueReportAsync();
            if (rep == null) return false;

            var (ss0, ss0t) = await lru.GetOrLoadAsync((rep.SetId, rep.SubmissionA), context, Load);
            var (ss1, ss1t) = await lru.GetOrLoadAsync((rep.SetId, rep.SubmissionB), context, Load);

            var result = Report.Generate(ss0t, ss1t);
            var frag = MatchReportCreate(ss0.ExternalId != result.SubmissionA.Id, result);
            await context.SaveReportAsync(rep, frag);
            return true;
        }

        protected override async Task ProcessAsync(IServiceScope scope, CancellationToken stoppingToken)
        {
            var context = scope.ServiceProvider.GetRequiredService<IJobContext>();
            var lru = new LruStore<(string, int), (Submission, Frontend.Submission)>();
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!await ResolveAsync(context, lru)) break;
            }

            lru.Clear();
        }
    }
}
