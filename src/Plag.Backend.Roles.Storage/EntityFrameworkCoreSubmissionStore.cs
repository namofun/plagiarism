using Microsoft.EntityFrameworkCore;
using Plag.Backend.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plag.Backend.Services
{
    public class EntityFrameworkCoreStoreService<TContext> : IStoreService
        where TContext : DbContext
    {
        public int PageCount { get; set; } = 30;

        public TContext Context { get; }

        public ICompileService Compile { get; }

        public DbSet<PlagiarismSet> Sets => Context.Set<PlagiarismSet>();

        public DbSet<Report> Reports => Context.Set<Report>();

        public DbSet<Entities.Submission> Submissions => Context.Set<Entities.Submission>();

        public EntityFrameworkCoreStoreService(TContext context, ICompileService compile)
        {
            Context = context;
            Compile = compile;
        }

        public async Task<PlagiarismSet> CreateSetAsync(string name)
        {
            var item = Sets.Add(new PlagiarismSet
            {
                Name = name,
                CreateTime = DateTimeOffset.Now,
            });

            await Context.SaveChangesAsync();
            return item.Entity;
        }

        public Task<Report> FindReportAsync(string id)
        {
            return Reports
                .Where(r => r.Id == id)
                .SingleOrDefaultAsync();
        }

        public Task<PlagiarismSet> FindSetAsync(string id)
        {
            return Sets
                .Where(r => r.Id == id)
                .SingleOrDefaultAsync();
        }

        public Task<Entities.Submission> FindSubmissionAsync(string id, bool includeFiles = true)
        {
            return Submissions.Where(s => s.Id == id)
                .IncludeIf(includeFiles, s => s.Files)
                .SingleOrDefaultAsync();
        }

        public Task<Compilation> GetCompilationAsync(string submitId)
        {
            return Context.Set<Compilation>()
                .Where(s => s.Id == submitId)
                .SingleOrDefaultAsync();
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

        public async Task<PagedViewList<PlagiarismSet>> ListSetsAsync(int page)
        {
            var count = await Sets.CountAsync();
            var content = await Sets
                .OrderByDescending(s => s.Id)
                .Skip((page - 1) * PageCount)
                .Take(PageCount)
                .ToListAsync();

            return new PagedViewList<PlagiarismSet>(content, page, count, PageCount);
        }

        public Task<List<Entities.Submission>> ListSubmissionsAsync(string setId)
        {
            return Submissions
                .Where(s => s.SetId == setId)
                .ToListAsync();
        }

        public async Task SubmitAsync(Entities.Submission submission)
        {
            Submissions.Add(submission);
            await Context.SaveChangesAsync();
        }

        public Task<List<Comparison>> GetComparisonsBySubmissionAsync(string submitId)
        {
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
    }
}
