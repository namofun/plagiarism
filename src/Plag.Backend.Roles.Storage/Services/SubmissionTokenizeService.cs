using Microsoft.EntityFrameworkCore;
using SatelliteSite.Data;
using SatelliteSite.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SatelliteSite.Services
{
    public class SubmissionTokenizeServiceBase<T> : ContextNotifyService<T>
    {
        public SubmissionTokenizeServiceBase(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        private async Task<PlagiarismSubmission> ResolveAsync(PlagiarismContext dbContext)
        {
            var ss = await dbContext.Submissions
                .Where(s => s.TokenProduced == null)
                .FirstOrDefaultAsync();
            if (ss == null) return null;

            var files = await dbContext.Set<PlagiarismFile>()
                .Where(s => s.SubmissionId == ss.Id)
                .ToListAsync();

            var lang = PdsRegistry.SupportedLanguages[ss.Language];
            var file = new SubmissionFileProxy(files);

            var tokens = new Plag.Submission(lang, file, ss.Id);
            ss.TokenProduced = !tokens.IL.Errors;

            var ce = await dbContext.Set<PlagiarismCompilation>()
                .Where(c => c.Id == ss.Id)
                .SingleOrDefaultAsync();

            if (ce == null)
            {
                ce = new PlagiarismCompilation { Id = ss.Id };
                dbContext.Set<PlagiarismCompilation>().Add(ce);
            }
            else
            {
                dbContext.Set<PlagiarismCompilation>().Update(ce);
            }

            if (!tokens.IL.Errors)
            {
                ce.Tokens = PdsRegistry.Serialize(tokens.IL);
                ce.Error = "";
            }
            else
            {
                ce.Tokens = null;
                ce.Error =
                    $"ANTLR4 failed with {tokens.IL.ErrorsCount} errors.\r\n"
                    + tokens.IL.ErrorInfo.ToString();
            }

            dbContext.Set<PlagiarismCompilation>().Add(ce);
            dbContext.Submissions.Update(ss);
            await dbContext.SaveChangesAsync();
            return ss;
        }

        private async Task ScheduleAsync(PlagiarismContext dbContext, PlagiarismSubmission ss)
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
                updateExpression: (s1, s2) => new PlagiarismReport { Pending = true },
                insertExpression: s => new PlagiarismReport
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
                updateExpression: (s1, s2) => new PlagiarismReport { Pending = true },
                insertExpression: s => new PlagiarismReport
                {
                    Pending = true,
                    SubmissionA = s.A,
                    SubmissionB = s.Id,
                });

            int tot = a + b;
            await dbContext.CheckSets
                .Where(c => c.Id == ss.SetId)
                .BatchUpdateAsync(c => new PlagiarismSet
                {
                    ReportCount = c.ReportCount + tot,
                    ReportPending = c.ReportPending + tot,
                });

            ReportGenerationService.Notify();
        }

        protected override async Task ProcessAsync(PlagiarismContext context, CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var s = await ResolveAsync(context);
                if (s != null)
                    await ScheduleAsync(context, s);
                else
                    break;
            }
        }
    }

    public class SubmissionTokenizeService : SubmissionTokenizeServiceBase<SubmissionTokenizeService>
    {
        public SubmissionTokenizeService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
