using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

        public DbSet<SubmissionFile<Guid>> Files => Context.Set<SubmissionFile<Guid>>();

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

        public override Task<List<PlagiarismSet<Guid>>> ListSetsAsync(
            int? cid = null,
            int? uid = null,
            int? skip = null,
            int? limit = null,
            bool asc = false)
        {
            return Sets.AsNoTracking()
                .WhereIf(cid.HasValue, s => s.ContestId == cid)
                .WhereIf(uid.HasValue, s => s.UserId == uid)
                .OrderByDescending(s => s.Id)
                .SkipIf(skip).TakeIf(limit)
                .OrderBy(s => s.Id, asc, true)
                .ToListAsync();
        }

        public override Task<List<Submission<Guid>>> ListSubmissionsAsync(
            Guid setid,
            string language = null,
            int? exclusive_category = null,
            int? inclusive_category = null,
            double? min_percent = null,
            int? skip = null,
            int? limit = null,
            string order = "id",
            bool asc = true)
        {
            language = language?.Trim();
            order = (order ?? "id").ToLowerInvariant();
            if (order != "id" && order != "percent")
            {
                throw new ArgumentOutOfRangeException();
            }

            return Submissions.AsNoTracking()
                .Where(s => s.SetId == setid)
                .WhereIf(!string.IsNullOrEmpty(language), s => s.Language == language)
                .WhereIf(exclusive_category.HasValue, s => s.ExclusiveCategory == exclusive_category)
                .WhereIf(inclusive_category.HasValue, s => s.InclusiveCategory == inclusive_category)
                .WhereIf(min_percent.HasValue, s => s.MaxPercent >= min_percent)
                .OrderBy(s => s.Id, asc, order == "id")
                .OrderBy(s => s.MaxPercent, asc, order == "percent")
                .SkipIf(skip).TakeIf(limit)
                .Select(Submission<Guid>.Minify)
                .ToListAsync();
        }

        public virtual async Task<int> GetUpcomingIdAsync(Guid setId, bool upwards)
        {
            var baseQuery = Submissions
                .Where(s => s.SetId == setId)
                .Select(s => (int?)s.Id);

            return upwards
                ? Math.Max(1, 1 + await baseQuery.MaxAsync() ?? 0) // maxid < 0 ? fix to 1
                : Math.Min(-1, -1 + await baseQuery.MinAsync() ?? 0); // minid > 0 ? fix to -1
        }

        private EntityEntry<Submission<Guid>> AddCore(Guid setId, int id, SubmissionCreation submission)
        {
            var extId = SequentialGuidGenerator.Create(Context);

            var entry = Submissions.Add(new Submission<Guid>
            {
                SetId = setId,
                Id = id,
                ExternalId = extId,
                ExclusiveCategory = submission.ExclusiveCategory ?? id,
                InclusiveCategory = submission.InclusiveCategory,
                Language = submission.Language,
                Name = submission.Name,
                UploadTime = DateTimeOffset.Now,
            });

            Files.AddRange(submission.Files.Select((i, j) => new SubmissionFile<Guid>
            {
                FileId = j + 1,
                Content = i.Content,
                FileName = i.FileName,
                FilePath = i.FilePath,
                SubmissionId = extId,
            }));

            return entry;
        }

        public override async Task<Submission<Guid>> SubmitAsync(Guid setId, SubmissionCreation submission)
        {
            var set = await FindSetAsync(setId);
            if (set == null) throw new ArgumentOutOfRangeException(nameof(setId), "Set not found.");

            if (submission.Id.HasValue)
            {
                var idd = submission.Id.Value;
                if (await Submissions.Where(s => s.SetId == setId && s.Id == idd).AnyAsync())
                {
                    throw new ArgumentOutOfRangeException(nameof(setId), "Duplicate submission ID.");
                }
            }

            var submissionId = submission.Id ?? await GetUpcomingIdAsync(setId, !set.ContestId.HasValue);
            var e = AddCore(setId, submissionId, submission);
            await Context.SaveChangesAsync();

            await Sets
                .Where(s => s.Id == setId)
                .BatchUpdateAsync(s => new PlagiarismSet<Guid>
                {
                    SubmissionCount = s.SubmissionCount + 1,
                });

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
                    ExclusiveCategory = s.ExclusiveCategory,
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
                    ExclusiveCategory = s.ExclusiveCategory,
                };

            return await reportA.Concat(reportB).ToListAsync();
        }

        public override async Task CompileAsync(Guid setid, int submitId, string error, byte[] result)
        {
            bool tokenProduced = result != null;
            await Submissions
                .Where(s => s.SetId == setid && s.Id == submitId)
                .BatchUpdateAsync(s => new Submission<Guid>
                {
                    Error = error,
                    TokenProduced = tokenProduced,
                    Tokens = result,
                });

            var (a, b) = tokenProduced ? (1, 0) : (0, 1);
            await Sets
                .Where(s => s.Id == setid)
                .BatchUpdateAsync(s => new PlagiarismSet<Guid>
                {
                    SubmissionSucceeded = s.SubmissionSucceeded + a,
                    SubmissionFailed = s.SubmissionFailed + b,
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
            model.Files = await GetFilesAsync(s.ExternalId);
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

        public override async Task<IReadOnlyList<SubmissionFile>> GetFilesAsync(Guid extId)
        {
            return await Files.AsNoTracking()
                .Where(s => s.SubmissionId == extId)
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

        public override async Task RescueAsync()
        {
            var reportAggregate = Reports
                .GroupBy(r => r.SetId)
                .Select(g => new { Id = g.Key, Total = g.Count(), Pending = g.Sum(a => a.Finished != true ? 1 : 0) });

            await Sets.BatchUpdateJoinAsync(
                inner: reportAggregate,
                outerKeySelector: s => s.Id,
                innerKeySelector: s => s.Id,
                updateSelector: (_, r) => new PlagiarismSet<Guid>
                {
                    ReportCount = r.Total,
                    ReportPending = r.Pending,
                });

            var submissionAggregate = Submissions
                .GroupBy(r => r.SetId)
                .Select(g => new { Id = g.Key, Total = g.Count(), Succ = g.Sum(a => a.TokenProduced == true ? 1 : 0), Fail = g.Sum(a => a.TokenProduced == false ? 1 : 0) });

            await Sets.BatchUpdateJoinAsync(
                inner: submissionAggregate,
                outerKeySelector: s => s.Id,
                innerKeySelector: s => s.Id,
                updateSelector: (_, r) => new PlagiarismSet<Guid>
                {
                    SubmissionCount = r.Total,
                    SubmissionSucceeded = r.Succ,
                    SubmissionFailed = r.Fail,
                });

            Signal1.Notify();
            Signal2.Notify();
        }

        public override ServiceVersion GetVersion()
        {
            return new ServiceVersion
            {
                FrontendVersion = typeof(Frontend.ILanguage).Assembly.GetName().Version.ToString(),
                BackendVersion = typeof(Backend.IBackendRoleStrategy).Assembly.GetName().Version.ToString(),
                Role = "relational_storage"
            };
        }
    }

    internal static class OrderByQueryableExtensions
    {
        public static IQueryable<TSource> OrderBy<TSource, TKey>(
            this IQueryable<TSource> source,
            System.Linq.Expressions.Expression<Func<TSource, TKey>> orderby,
            bool ascending,
            bool used)
        {
            return !used
                ? source
                : ascending
                ? source.OrderBy(orderby)
                : source.OrderByDescending(orderby);
        }
    }
}
