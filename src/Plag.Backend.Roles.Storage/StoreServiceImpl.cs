using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Plag.Backend.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plag.Backend.Services
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
    public class EntityFrameworkCoreStoreService<TContext> :
        IStoreExtService, IDisposable
        where TContext : DbContext
    {
        public int PageCount { get; set; } = 30;

        public TContext Context { get; }

        public ICompileService Compile { get; }

        public DbSet<PlagiarismSet> Sets => Context.Set<PlagiarismSet>();

        public DbSet<Report> Reports => Context.Set<Report>();

        public DbSet<Submission> Submissions => Context.Set<Submission>();

        public IMemoryCache TransientStore { get; }

        public EntityFrameworkCoreStoreService(TContext context, ICompileService compile)
        {
            Context = context;
            Compile = compile;
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            TransientStore = new MemoryCache(new MemoryCacheOptions { Clock = new SystemClock() });
        }

        private Task<List<T>> NotFoundList<T>()
        {
            return Task.FromResult(new List<T>());
        }

        private Task<T> NotFound<T>() where T : class
        {
            return Task.FromResult<T>(null);
        }

        public async Task<PlagiarismSet> CreateSetAsync(string name)
        {
            var item = Sets.Add(new PlagiarismSet
            {
                Name = name,
                CreateTime = DateTimeOffset.Now,
                Id = Guid.NewGuid().ToString()
            });

            await Context.SaveChangesAsync();
            return item.Entity;
        }

        public Task<Report> FindReportAsync(string id)
        {
            if (!Guid.TryParse(id, out _)) return NotFound<Report>();

            return Reports
                .Where(r => r.Id == id)
                .SingleOrDefaultAsync();
        }

        public Task<PlagiarismSet> FindSetAsync(string id)
        {
            if (!Guid.TryParse(id, out _)) return NotFound<PlagiarismSet>();

            return Sets
                .Where(r => r.Id == id)
                .SingleOrDefaultAsync();
        }

        public Task<Submission> FindSubmissionAsync(string id, bool includeFiles = true)
        {
            if (!Guid.TryParse(id, out _)) return NotFound<Submission>();

            return Submissions.Where(s => s.Id == id)
                .IncludeIf(includeFiles, s => s.Files)
                .SingleOrDefaultAsync();
        }

        public Task<Compilation> GetCompilationAsync(string submitId)
        {
            if (!Guid.TryParse(submitId, out _)) return NotFound<Compilation>();

            return Context.Set<Compilation>()
                .FindAsync(submitId)
                .AsTask();
        }

        public Task<LanguageInfo> FindLanguageAsync(string langName)
        {
            var lang = Compile.FindLanguage(langName);
            if (lang == null) return Task.FromResult<LanguageInfo>(null);
            return Task.FromResult(new LanguageInfo(lang.Name, lang.ShortName, lang.Suffixes));
        }

        public Task<List<LanguageInfo>> ListLanguageAsync()
        {
            return Task.FromResult(
                AntlrCompileService.SupportedLanguages.Values
                    .Select(a => new LanguageInfo(a.Name, a.ShortName, a.Suffixes))
                    .ToList());
        }

        public Task<List<PlagiarismSet>> ListSetsAsync(int? skip, int? take)
        {
            IQueryable<PlagiarismSet> q = Sets.OrderByDescending(s => s.Id);
            if (skip.HasValue) q = q.Skip(skip.Value);
            if (take.HasValue) q = q.Take(take.Value);
            return q.ToListAsync();
        }

        public Task<List<Submission>> ListSubmissionsAsync(string setId)
        {
            if (!Guid.TryParse(setId, out _)) return NotFoundList<Submission>();

            return Submissions
                .Where(s => s.SetId == setId)
                .ToListAsync();
        }

        public async Task<Submission> SubmitAsync(SubmissionCreation submission)
        {
            var id = Guid.NewGuid().ToString();

            var files = submission.Files.Select((i, j) => new SubmissionFile
            {
                FileId = j + 1,
                Content = i.Content,
                FileName = i.FileName,
                FilePath = i.FilePath,
                SubmissionId = id
            });

            var e = Submissions.Add(new Submission
            {
                Files = files.ToList(),
                Id = id,
                Language = submission.Language,
                Name = submission.Name,
                SetId = submission.SetId,
                UploadTime = DateTimeOffset.Now,
            });

            await Context.SaveChangesAsync();
            SubmissionTokenizeService.Notify();
            return e.Entity;
        }

        public Task<List<Comparison>> GetComparisonsBySubmissionAsync(string submitId)
        {
            if (!Guid.TryParse(submitId, out _)) return NotFoundList<Comparison>();

            var reportA =
                from r in Reports
                where r.SubmissionB == submitId
                join s in Submissions on r.SubmissionA equals s.Id
                select new Comparison
                {
                    BiggestMatch = r.BiggestMatch,
                    SubmissionAnother = s.Name,
                    SubmissionIdAnother = s.Id,
                    Id = r.Id,
                    Pending = r.Pending,
                    TokensMatched = r.TokensMatched,
                    Percent = r.Percent,
                    PercentIt = r.PercentA,
                    PercentSelf = r.PercentB
                };

            var reportB =
                from r in Reports
                where r.SubmissionA == submitId
                join s in Submissions on r.SubmissionB equals s.Id
                select new Comparison
                {
                    BiggestMatch = r.BiggestMatch,
                    SubmissionAnother = s.Name,
                    SubmissionIdAnother = s.Id,
                    Id = r.Id,
                    Pending = r.Pending,
                    TokensMatched = r.TokensMatched,
                    Percent = r.Percent,
                    PercentIt = r.PercentB,
                    PercentSelf = r.PercentA
                };

            return reportA.Concat(reportB).ToListAsync();
        }

        public async Task CompileAsync(Submission submitId, string error, byte[] result)
        {
            submitId.TokenProduced = result != null;
            Context.Set<Submission>().Update(submitId);
            var ce = await Context.Set<Compilation>().FindAsync(submitId.Id);

            if (ce == null)
            {
                ce = new Compilation
                {
                    Id = submitId.Id,
                    Error = error,
                    Tokens = result
                };

                Context.Set<Compilation>().Add(ce);
            }
            else
            {
                ce.Tokens = result;
                ce.Error = error;
                Context.Set<Compilation>().Update(ce);
            }

            await Context.SaveChangesAsync();
        }

        public Task<Submission> FetchAsync()
        {
            return Submissions
                .Where(s => s.TokenProduced == null)
                .Include(s => s.Files)
                .FirstOrDefaultAsync();
        }

        public async Task ScheduleAsync(Submission ss)
        {
            if (ss.TokenProduced != true) return;

            var leftQuery =
                from s in Submissions
                where s.SetId == ss.SetId && s.Language == ss.Language
                where s.TokenProduced == true && string.Compare(s.Id, ss.Id) == -1
                select new { s.Id, B = ss.Id };

            var rightQuery =
                from s in Submissions
                where s.SetId == ss.SetId && s.Language == ss.Language
                where s.TokenProduced == true && string.Compare(s.Id, ss.Id) == 1
                select new { s.Id, A = ss.Id };

            int a = await Reports.MergeAsync(
                sourceTable: leftQuery,
                targetKey: r => new { Id = r.SubmissionA, B = r.SubmissionB },
                sourceKey: r => new { r.Id, r.B },
                delete: false,
                updateExpression: (s1, s2) => new Report { Pending = true },
                insertExpression: s => new Report
                {
                    Pending = true,
                    SubmissionA = s.Id,
                    SubmissionB = s.B,
                });

            int b = await Reports.MergeAsync(
                sourceTable: rightQuery,
                targetKey: r => new { A = r.SubmissionA, Id = r.SubmissionB },
                sourceKey: r => new { r.A, r.Id },
                delete: false,
                updateExpression: (s1, s2) => new Report { Pending = true },
                insertExpression: s => new Report
                {
                    Pending = true,
                    SubmissionA = s.A,
                    SubmissionB = s.Id,
                });

            int tot = a + b;
            await Sets.Where(c => c.Id == ss.SetId)
                .BatchUpdateAsync(c => new PlagiarismSet
                {
                    ReportCount = c.ReportCount + tot,
                    ReportPending = c.ReportPending + tot,
                });
        }

        public ValueTask<Submission> QuickFindSubmissionAsync(string submitId)
        {
            if (!Guid.TryParse(submitId, out _)) return new ValueTask<Submission>(default(Submission));

            return Submissions.FindAsync(submitId);
        }

        public Task<Report> FetchOneAsync()
        {
            return Reports.Where(r => r.Pending).FirstOrDefaultAsync();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
        public void Dispose()
        {
            TransientStore.Dispose();
        }

        public Task<List<SubmissionFile>> GetFilesAsync(string submitId)
        {
            if (!Guid.TryParse(submitId, out _)) return NotFoundList<SubmissionFile>();

            return Context.Set<SubmissionFile>()
                .Where(s => s.SubmissionId == submitId)
                .OrderBy(s => s.FileId)
                .ToListAsync();
        }

        public async Task SaveReportAsync(string setId, Report rep)
        {
            if (!Guid.TryParse(setId, out _)) throw new InvalidOperationException();

            Reports.Update(rep);
            await Context.SaveChangesAsync();

            await Sets
                .Where(c => c.Id == setId)
                .BatchUpdateAsync(c => new PlagiarismSet { ReportPending = c.ReportPending - 1 });

            var sids = new[] { rep.SubmissionA, rep.SubmissionB };
            await Submissions
                .Where(c => sids.Contains(c.Id) && c.MaxPercent < rep.Percent)
                .BatchUpdateAsync(c => new Submission { MaxPercent = rep.Percent });
        }
    }
}
