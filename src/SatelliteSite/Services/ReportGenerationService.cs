using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SatelliteSite.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SatelliteSite.Services
{
    public class ReportGenerationServiceBase<T> : ContextNotifyService<T>
    {
        public ReportGenerationServiceBase(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        private async ValueTask<Submission> GetOrLoadAsync(PlagiarismContext dbContext, int id)
        {
            var e = dbContext.ChangeTracker
                .Entries<Submission>()
                .Where(s => s.Entity?.Id == id)
                .SingleOrDefault();
            if (e != null) return e.Entity;

            var s = await dbContext.Submissions
                .Where(s => s.TokenProduced == true && s.Id == id)
                .SingleOrDefaultAsync();
            if (s == null) throw new InvalidDataException();

            var lang = PdsRegistry.SupportedLanguages[s.Language];
            if (!File.Exists($"Tokens/s{s.Id}.bin"))
            {
                Logger.LogWarning($"Token file `s{s.Id}.bin` missing, generating at once.");
                s.Files = await dbContext.Set<SubmissionFile>()
                    .Where(s => s.SubmissionId == id)
                    .ToListAsync();
                s.Tokens = new Plag.Submission(lang, new SubmissionFileProxy(s), s.Id);
            }
            else
            {
                using var fs = new FileStream($"Tokens/s{s.Id}.bin", FileMode.Open);
                var tokens = await PdsRegistry.DeserializeAsync(fs, lang);
                s.Tokens = new Plag.Submission(lang, null, s.Id, tokens);
            }

            return s;
        }

        private static void MatchReportCreate(MatchReport report, Plag.Matching matching)
        {
            bool swapAB = report.SubmissionA != matching.SubmissionA.Id;
            report.TokensMatched = matching.TokensMatched;
            report.BiggestMatch = matching.BiggestMatch;
            report.Percent = matching.Percent;
            report.Pending = false;

            if (swapAB)
            {
                report.PercentB = matching.PercentA;
                report.PercentA = matching.PercentB;

                report.MatchPairs = matching.Select((i, j) => new MatchPair
                {
                    MatchingId = j,
                    ContentStartB = matching.SubmissionA.IL[i.StartA].Column,
                    ContentEndB = matching.SubmissionA.IL[i.StartA + i.Length].Column,
                    ContentStartA = matching.SubmissionB.IL[i.StartB].Column,
                    ContentEndA = matching.SubmissionB.IL[i.StartB + i.Length].Column,
                    FileB = matching.SubmissionA.IL[i.StartA].FileId,
                    FileA = matching.SubmissionB.IL[i.StartB].FileId,
                })
                .ToList();
            }
            else
            {
                report.PercentA = matching.PercentA;
                report.PercentB = matching.PercentB;

                report.MatchPairs = matching.Select((i, j) => new MatchPair
                {
                    MatchingId = j,
                    ContentStartA = matching.SubmissionA.IL[i.StartA].Column,
                    ContentEndA = matching.SubmissionA.IL[i.StartA + i.Length].Column,
                    ContentStartB = matching.SubmissionB.IL[i.StartB].Column,
                    ContentEndB = matching.SubmissionB.IL[i.StartB + i.Length].Column,
                    FileA = matching.SubmissionA.IL[i.StartA].FileId,
                    FileB = matching.SubmissionB.IL[i.StartB].FileId,
                })
                .ToList();
            }
        }

        private async Task<MatchReport> ResolveAsync(PlagiarismContext dbContext)
        {
            var rep = await dbContext.Reports
                .Where(s => s.Pending)
                .FirstOrDefaultAsync();
            if (rep == null) return null;

            var ss0 = await GetOrLoadAsync(dbContext, rep.SubmissionA);
            var ss1 = await GetOrLoadAsync(dbContext, rep.SubmissionB);

            var lang = PdsRegistry.SupportedLanguages[ss0.Language];
            var result = Plag.GSTiling.Compare(ss0.Tokens, ss1.Tokens, lang.MinimalTokenMatch);

            MatchReportCreate(rep, result);
            dbContext.Reports.Update(rep);
            await dbContext.SaveChangesAsync();

            await dbContext.CheckSets
                .Where(c => c.Id == ss0.SetId)
                .BatchUpdateAsync(c => new CheckSet { ReportPending = c.ReportPending - 1 });
            var sids = new[] { ss0.Id, ss1.Id };
            await dbContext.Submissions
                .Where(c => sids.Contains(c.Id) && c.MaxPercent < rep.Percent)
                .BatchUpdateAsync(c => new Submission { MaxPercent = rep.Percent });

            return rep;
        }

        protected override async Task ProcessAsync(PlagiarismContext context, CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var s = await ResolveAsync(context);
                if (s == null) break;
            }
        }
    }

    public class ReportGenerationService : ReportGenerationServiceBase<ReportGenerationService>
    {
        public ReportGenerationService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
