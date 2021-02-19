using Microsoft.EntityFrameworkCore;
using Plag.Backend.Entities;
using Plag.Backend.Jobs;
using Plag.Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plag.Backend.Services
{
    public class EntityFrameworkCoreStoreService<TContext> : PdsServiceBase<Guid>
        where TContext : DbContext
    {
        public int PageCount { get; set; } = 30;

        public TContext Context { get; }

        public ICompileService Compile { get; }

        public DbSet<PlagiarismSet<Guid>> Sets => Context.Set<PlagiarismSet<Guid>>();

        public DbSet<Report<Guid>> Reports => Context.Set<Report<Guid>>();

        public DbSet<Submission<Guid>> Submissions => Context.Set<Submission<Guid>>();

        public IResettableSignal<SubmissionTokenizeService> Signal1 { get; }

        public IResettableSignal<ReportGenerationService> Signal2 { get; }

        public EntityFrameworkCoreStoreService(
            TContext context,
            ICompileService compile,
            IResettableSignal<SubmissionTokenizeService> signal1,
            IResettableSignal<ReportGenerationService> signal2)
        {
            Context = context;
            Compile = compile;
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            Signal1 = signal1;
            Signal2 = signal2;
        }

        public override bool TryGetKey(string id, out Guid key)
        {
            return Guid.TryParse(id, out key);
        }

        public override async Task<PlagiarismSet<Guid>> CreateSetAsync(SetCreation metadata)
        {
            var item = Sets.Add(new PlagiarismSet<Guid>
            {
                Name = metadata.Name,
                ContestId = metadata.ContestId,
                UserId = metadata.UserId,
                CreateTime = DateTimeOffset.Now,
                Id = SequentialGuidGenerator.Create(Context),
            });

            await Context.SaveChangesAsync();
            return item.Entity;
        }

        public override Task<Report<Guid>> FindReportAsync(Guid id)
        {
            return Reports.AsNoTracking()
                .Where(r => r.ExternalId == id)
                .SingleOrDefaultAsync();
        }

        public override Task<PlagiarismSet<Guid>> FindSetAsync(Guid id)
        {
            return Sets.AsNoTracking()
                .Where(r => r.Id == id)
                .SingleOrDefaultAsync();
        }

        public override Task<Submission<Guid>> FindSubmissionAsync(Guid setid, int submitid)
        {
            return Submissions.AsNoTracking()
                .Where(c => c.SetId == setid && c.Id == submitid)
                .Select(Submission<Guid>.Minify)
                .SingleOrDefaultAsync();
        }

        public override Task<Compilation> GetCompilationAsync(Guid setid, int submitid)
        {
            return Submissions.AsNoTracking()
                .Where(c => c.SetId == setid && c.Id == submitid)
                .Select(c => new Compilation { Error = c.Error, Tokens = c.Tokens })
                .SingleOrDefaultAsync();
        }

        public override Task<LanguageInfo> FindLanguageAsync(string langName)
        {
            var lang = Compile.FindLanguage(langName);
            if (lang == null) return Task.FromResult<LanguageInfo>(null);
            return Task.FromResult(new LanguageInfo(lang.Name, lang.ShortName, lang.Suffixes));
        }

        public override Task<List<LanguageInfo>> ListLanguageAsync()
        {
            return Task.FromResult(
                Compile.GetLanguages()
                    .Select(a => new LanguageInfo(a.Name, a.ShortName, a.Suffixes))
                    .ToList());
        }

        public override Task<List<PlagiarismSet<Guid>>> ListSetsAsync(int? skip = null, int? limit = null)
        {
            return Sets.AsNoTracking()
                .OrderByDescending(s => s.Id)
                .SkipIf(skip).TakeIf(limit)
                .ToListAsync();
        }

        public override Task<List<Submission<Guid>>> ListSubmissionsAsync(
            Guid setid,
            int? exclusive_category,
            int? inclusive_category,
            double? min_percent)
        {
            return Submissions.AsNoTracking()
                .Where(s => s.SetId == setid)
                .WhereIf(exclusive_category.HasValue, s => s.ExclusiveCategory == exclusive_category)
                .WhereIf(inclusive_category.HasValue, s => s.InclusiveCategory == inclusive_category)
                .WhereIf(min_percent.HasValue, s => s.MaxPercent >= min_percent)
                .Select(Submission<Guid>.Minify)
                .ToListAsync();
        }

        public override async Task<Submission<Guid>> SubmitAsync(Guid setId, SubmissionCreation submission)
        {
            var id = SequentialGuidGenerator.Create(Context);
            var submissionId = submission.Id ??
                (await Submissions.Where(s => s.SetId == setId).CountAsync() + 1);

            var e = Submissions.Add(new Submission<Guid>
            {
                SetId = setId,
                Id = submissionId,
                ExternalId = id,
                ExclusiveCategory = submission.ExclusiveCategory ?? submissionId,
                InclusiveCategory = submission.InclusiveCategory,
                Language = submission.Language,
                Name = submission.Name,
                UploadTime = DateTimeOffset.Now,
            });

            Context.Set<SubmissionFile<Guid>>()
                .AddRange(submission.Files.Select((i, j) => new SubmissionFile<Guid>
                {
                    FileId = j + 1,
                    Content = i.Content,
                    FileName = i.FileName,
                    FilePath = i.FilePath,
                    SubmissionId = submissionId,
                    SetId = setId,
                }));

            await Context.SaveChangesAsync();
            Signal1.Notify();
            return e.Entity;
        }

        private class HoistedComparison : Comparison
        {
            public Guid ExternalId { get; set; }

            public override string Id { get => ExternalId.ToString(); set { } }
        }

        public override async Task<IReadOnlyList<Comparison>> GetComparisonsBySubmissionAsync(Guid setid, int submitid)
        {
            var reportA =
                from r in Reports
                where r.SetId == setid && r.SubmissionB == submitid
                join s in Submissions on new { r.SetId, Id = r.SubmissionA } equals new { s.SetId, s.Id }
                select new HoistedComparison
                {
                    BiggestMatch = r.BiggestMatch,
                    SubmissionIdAnother = r.SubmissionA,
                    SubmissionNameAnother = s.Name,
                    Finished = r.Finished,
                    TokensMatched = r.TokensMatched,
                    Percent = r.Percent,
                    PercentIt = r.PercentA,
                    PercentSelf = r.PercentB,
                    ExternalId = r.ExternalId,
                };

            var reportB =
                from r in Reports
                where r.SetId == setid && r.SubmissionA == submitid
                join s in Submissions on new { r.SetId, Id = r.SubmissionB } equals new { s.SetId, s.Id }
                select new HoistedComparison
                {
                    BiggestMatch = r.BiggestMatch,
                    SubmissionIdAnother = r.SubmissionB,
                    SubmissionNameAnother = s.Name,
                    Finished = r.Finished,
                    TokensMatched = r.TokensMatched,
                    Percent = r.Percent,
                    PercentIt = r.PercentB,
                    PercentSelf = r.PercentA,
                    ExternalId = r.ExternalId,
                };

            return await reportA.Concat(reportB).ToListAsync();
        }

        public override Task CompileAsync(Guid setid, int submitId, string error, byte[] result)
        {
            bool tokenProduced = result != null;

            return Submissions
                .Where(s => s.SetId == setid && s.Id == submitId)
                .BatchUpdateAsync(s => new Submission<Guid>
                {
                    Error = error,
                    TokenProduced = tokenProduced,
                    Tokens = result,
                });
        }

        public override async Task<Submission> DequeueSubmissionAsync()
        {
            var s = await Submissions.AsNoTracking()
                .Where(s => s.TokenProduced == null)
                .Select(Submission<Guid>.Minify)
                .FirstOrDefaultAsync();

            if (s == null) return null;
            var model = s.ToModel();
            model.Files = await GetFilesAsync(s.SetId, s.Id);
            return model;
        }

        public override async Task ScheduleAsync(Guid setId, int submitId, int exclusive, int inclusive, string langId)
        {
            var baseQuery = Submissions
                .Where(s => s.SetId == setId && s.Language == langId)
                .Where(s => s.ExclusiveCategory != exclusive && s.InclusiveCategory == inclusive)
                .Where(s => s.TokenProduced == true);

            var twoQuery = Queryable.Concat(
                baseQuery.Where(s => s.Id < submitId).Select(s => new { S = s.SetId, A = submitId, B = s.Id }),
                baseQuery.Where(s => s.Id > submitId).Select(s => new { S = s.SetId, A = s.Id, B = submitId }));

            var affected = await Reports.UpsertAsync(
                sources: twoQuery,
                insertExpression: s => new Report<Guid> { Finished = null, SetId = s.S, SubmissionA = s.A, SubmissionB = s.B, ExternalId = Guid.NewGuid() },
                updateExpression: (_, __) => new Report<Guid> { Finished = null });

            await Sets
                .Where(c => c.Id == setId)
                .BatchUpdateAsync(c => new PlagiarismSet<Guid>
                {
                    ReportCount = c.ReportCount + affected,
                    ReportPending = c.ReportPending + affected,
                });
        }

        public override async Task<ReportTask> DequeueReportAsync()
        {
           var r = await Reports.AsNoTracking()
               .Where(r => r.Finished == null)
               .Select(r => new { r.ExternalId, r.SetId, r.SubmissionA, r.SubmissionB })
               .FirstOrDefaultAsync();

            if (r == null) return null;

            await Reports
                .Where(o => o.ExternalId == r.ExternalId)
                .BatchUpdateAsync(r => new Report<Guid> { Finished = false });

            return ReportTask.Of(r.ExternalId, r.SetId, r.SubmissionA, r.SubmissionB);
        }

        public override async Task<IReadOnlyList<SubmissionFile>> GetFilesAsync(Guid setId, int submitId)
        {
            return await Context.Set<SubmissionFile<Guid>>()
                .AsNoTracking()
                .Where(s => s.SetId == setId && s.SubmissionId == submitId)
                .OrderBy(s => s.FileId)
                .ToListAsync();
        }

        public override async Task SaveReportAsync(Guid setid, int a, int b, Guid extid, ReportFragment fragment)
        {
            await Reports
                .Where(r => r.SetId == setid && r.SubmissionA == a && r.SubmissionB == b)
                .BatchUpdateAsync(r => new Report<Guid>
                {
                    TokensMatched = fragment.TokensMatched,
                    BiggestMatch = fragment.BiggestMatch,
                    Finished = true,
                    Percent = fragment.Percent,
                    PercentA = fragment.PercentA,
                    PercentB = fragment.PercentB,
                    Matches = fragment.Matches,
                });

            await Sets
                .Where(c => c.Id == setid)
                .BatchUpdateAsync(c => new PlagiarismSet<Guid> { ReportPending = c.ReportPending - 1 });

            await Submissions
                .Where(c => c.SetId == setid && (c.Id == a || c.Id == b) && c.MaxPercent < fragment.Percent)
                .BatchUpdateAsync(c => new Submission<Guid> { MaxPercent = fragment.Percent });
        }

        public override Task RescueAsync()
        {
            Signal1.Notify();
            Signal2.Notify();
            return Task.CompletedTask;
        }

        public override object GetVersion()
        {
            var fronend_version = typeof(Frontend.ILanguage).Assembly.GetName().Version.ToString();
            var backend_version = typeof(Backend.IBackendRoleStrategy).Assembly.GetName().Version.ToString();
            return new { fronend_version, backend_version, role = "relational_storage" };
        }
    }
}
