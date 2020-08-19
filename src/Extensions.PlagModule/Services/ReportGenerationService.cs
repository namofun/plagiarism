using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SatelliteSite.Data;
using SatelliteSite.Entities;
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

        private async ValueTask<PlagiarismSubmission> GetOrLoadAsync(DbContext dbContext, int id)
        {
            var e = dbContext.ChangeTracker
                .Entries<PlagiarismSubmission>()
                .Where(s => s.Entity?.Id == id)
                .SingleOrDefault();
            if (e != null) return e.Entity;

            var s = await dbContext.Submissions
                .Where(s => s.TokenProduced == true && s.Id == id)
                .SingleOrDefaultAsync();
            if (s == null) throw new InvalidDataException();

            var c = await dbContext.Set<PlagiarismCompilation>()
                .AsNoTracking()
                .Where(s => s.Id == id)
                .SingleOrDefaultAsync();

            var lang = PdsRegistry.SupportedLanguages[s.Language];
            if (c?.Tokens == null)
            {
                Logger.LogWarning($"Token for s{s.Id} missing, generating at once.");
                s.Files = await dbContext.Set<PlagiarismFile>()
                    .Where(s => s.SubmissionId == id)
                    .ToListAsync();
                s.Tokens = new Plag.Submission(lang, new SubmissionFileProxy(s), s.Id);
            }
            else
            {
                var tokens = PdsRegistry.Deserialize(c.Tokens, lang);
                s.Tokens = new Plag.Submission(lang, null, s.Id, tokens);
            }

            return s;
        }

        private static void MatchReportCreate(PlagiarismReport report, Plag.Matching matching)
        {
            bool swapAB = report.SubmissionA != matching.SubmissionA.Id;
            report.TokensMatched = matching.TokensMatched;
            report.BiggestMatch = matching.BiggestMatch;
            report.Percent = matching.Percent;
            report.Pending = false;
            report.Matches = PdsRegistry.Serialize(matching, swapAB);
            (report.PercentA, report.PercentB) = swapAB
                ? (matching.PercentB, matching.PercentA)
                : (matching.PercentA, matching.PercentB);
        }

        private async Task<PlagiarismReport> ResolveAsync(PlagiarismContext dbContext)
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
                .BatchUpdateAsync(c => new PlagiarismSet { ReportPending = c.ReportPending - 1 });
            var sids = new[] { ss0.Id, ss1.Id };
            await dbContext.Submissions
                .Where(c => sids.Contains(c.Id) && c.MaxPercent < rep.Percent)
                .BatchUpdateAsync(c => new PlagiarismSubmission { MaxPercent = rep.Percent });

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
