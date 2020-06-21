using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SatelliteSite.Controllers;
using SatelliteSite.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SatelliteSite.Services
{
    public class SubmissionTokenizeService : BackgroundService
    {
        static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public IServiceProvider ServiceProvider { get; }

        public ILogger<SubmissionTokenizeService> Logger { get; }

        public static void Notify() => _semaphore.Release();

        public SubmissionTokenizeService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Logger = serviceProvider.GetRequiredService<ILogger<SubmissionTokenizeService>>();
        }

        private async Task<Submission> ResolveAsync(PlagiarismContext dbContext)
        {
            var ss = await dbContext.Submissions
                .Where(s => s.TokenProduced == null)
                .FirstOrDefaultAsync();
            if (ss == null) return null;

            var files = await dbContext.Set<SubmissionFile>()
                .Where(s => s.SubmissionId == ss.Id)
                .ToListAsync();

            var lang = PdsRegistry.SupportedLanguages[ss.Language]();
            var file = new SubmissionFileProxy(files);

            var tokens = new Plag.Submission(lang, file, ss.Id);

            ss.TokenProduced = !tokens.IL.Errors;

            if (!tokens.IL.Errors)
            {
                ss.Tokens ??= new List<Token>();
                for (var i = 0; i < tokens.IL.Size; i++)
                {
                    Token t = tokens.IL[i];
                    t.TokenId = i;
                    ss.Tokens.Add(t);
                }
            }

            dbContext.Submissions.Update(ss);
            await dbContext.SaveChangesAsync();
            return ss;
        }

        private async Task ScheduleAsync(PlagiarismContext dbContext, Submission ss)
        {
            if (ss.TokenProduced != true) return;

            var leftQuery =
                from s in dbContext.Submissions
                where s.SetId == ss.SetId && s.Language == ss.Language
                where s.TokenProduced == true && s.Id < ss.Id
                select new { s.Id, B = ss.Id };

            var rightQuery =
                from s in dbContext.Submissions
                where s.SetId == ss.SetId && s.Language == ss.Language
                where s.TokenProduced == true && s.Id > ss.Id
                select new { s.Id, A = ss.Id };

            int a = await dbContext.Reports.MergeAsync(
                sourceTable: leftQuery,
                targetKey: r => new { Id = r.SubmissionA, B = r.SubmissionB },
                sourceKey: r => new { r.Id, r.B },
                delete: false,
                updateExpression: (s1, s2) => new MatchReport { Pending = true },
                insertExpression: s => new MatchReport
                {
                    Pending = true,
                    SubmissionA = s.Id,
                    SubmissionB = s.B,
                });

            int b = await dbContext.Reports.MergeAsync(
                sourceTable: rightQuery,
                targetKey: r => new { A = r.SubmissionA, Id = r.SubmissionB },
                sourceKey: r => new { r.A, r.Id },
                delete: false,
                updateExpression: (s1, s2) => new MatchReport { Pending = true },
                insertExpression: s => new MatchReport
                {
                    Pending = true,
                    SubmissionA = s.A,
                    SubmissionB = s.Id,
                });

            int tot = a + b;
            await dbContext.CheckSets
                .Where(c => c.Id == ss.SetId)
                .BatchUpdateAsync(c => new CheckSet
                {
                    ReportCount = c.ReportCount + tot,
                    ReportPending = c.ReportPending + tot,
                });

            ReportGenerationService.Notify();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                await _semaphore.WaitAsync(stoppingToken);
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    using var scope = ServiceProvider.CreateScope();
                    using var dbContext = scope.ServiceProvider.GetRequiredService<PlagiarismContext>();

                    while (true)
                    {
                        var s = await ResolveAsync(dbContext);
                        if (s != null)
                            await ScheduleAsync(dbContext, s);
                        else
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "An error happened unexpected.");
                }
            }
        }
    }
}
