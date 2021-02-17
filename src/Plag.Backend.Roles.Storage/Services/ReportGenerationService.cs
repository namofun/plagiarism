using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plag.Backend.Entities;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Plag.Backend.Services
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

        private async ValueTask<(Submission, Frontend.Submission)> GetOrLoadAsync(IStoreExtService store, string id)
        {
            var s = await store.QuickFindSubmissionAsync(id);
            if (s == null || s.TokenProduced != true)
                throw new InvalidDataException();

            var ss = await store.TransientStore.GetOrCreateAsync(id, async entry =>
            {
                var c = await store.GetCompilationAsync(id);
                var lang = Compile.FindLanguage(s.Language);

                if (c?.Tokens == null)
                {
                    Logger.LogWarning($"Token for s{s.Id} missing, generating at once.");
                    var fs = await store.GetFilesAsync(s.Id);
                    return new Frontend.Submission(lang, new Frontend.SubmissionFileProxy(s), s.Id);
                }
                else
                {
                    var tokens = Convert.TokenDeserialize(c.Tokens, lang);
                    return new Frontend.Submission(lang, null, s.Id, tokens);
                }
            });

            return (s, ss);
        }

        private void MatchReportCreate(Report report, Frontend.Matching matching)
        {
            bool swapAB = report.SubmissionA != matching.SubmissionA.Id;
            report.TokensMatched = matching.TokensMatched;
            report.BiggestMatch = matching.BiggestMatch;
            report.Percent = matching.Percent;
            report.Pending = false;
            report.Matches = Convert.MatchSerialize(matching, swapAB);
            (report.PercentA, report.PercentB) = swapAB
                ? (matching.PercentB, matching.PercentA)
                : (matching.PercentA, matching.PercentB);
        }

        private async Task<Report> ResolveAsync(IStoreExtService store)
        {
            var rep = await store.FetchOneAsync();
            if (rep == null) return null;

            var (ss0, ss0t) = await GetOrLoadAsync(store, rep.SubmissionA);
            var (ss1, ss1t) = await GetOrLoadAsync(store, rep.SubmissionB);

            var result = Report.Generate(ss0t, ss1t);
            MatchReportCreate(rep, result);
            await store.SaveReportAsync(ss0.SetId, rep);
            return rep;
        }

        protected override async Task ProcessAsync(
            IStoreExtService context,
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var s = await ResolveAsync(context);
                if (s == null) break;
            }
        }
    }
}
