﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Entities;
using Xylab.PlagiarismDetect.Backend.Models;

namespace Xylab.PlagiarismDetect.Backend.Services
{
    public class EntityFrameworkCoreStoreService<TContext> : PdsServiceBase<Guid>
        where TContext : DbContext
    {
        private const string LanguageListConfigurationName = "pds_language_list";
        private readonly IConfigurationRegistry _configurationRegistry;
        private readonly ILanguageProvider _languageProvider;
        private readonly ISignalProvider _signalProvider;
        private readonly SequentialGuidGenerator _guidGenerator;

        public TContext Context { get; }

        public DbSet<PlagiarismSet<Guid>> Sets => Context.Set<PlagiarismSet<Guid>>();

        public DbSet<Report<Guid>> Reports => Context.Set<Report<Guid>>();

        public DbSet<Submission<Guid>> Submissions => Context.Set<Submission<Guid>>();

        public DbSet<SubmissionFile<Guid>> Files => Context.Set<SubmissionFile<Guid>>();

        public EntityFrameworkCoreStoreService(
            TContext context,
            IEnumerable<ILanguageProvider> languageProvider,
            IConfigurationRegistry configurationRegistry,
            ISignalProvider signalProvider,
            SequentialGuidGenerator<TContext> sequentialGuidGenerator)
        {
            _languageProvider = languageProvider.SingleOrDefault();
            _configurationRegistry = configurationRegistry;
            _signalProvider = signalProvider;
            _guidGenerator = sequentialGuidGenerator;

            Context = context;
            context.ChangeTracker.AutoDetectChangesEnabled = false;
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
                Id = _guidGenerator.Create(),
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

        public override async Task ResetCompilationAsync(Guid setid, int submitid)
        {
            var cur = await Submissions
                .Where(c => c.SetId == setid && c.Id == submitid)
                .Select(a => new { a.TokenProduced })
                .FirstOrDefaultAsync();

            if (cur == null) throw new KeyNotFoundException();
            if (cur.TokenProduced == null) return;
            var (a, b) = cur.TokenProduced.Value ? (1, 0) : (0, 1);

            await Submissions
                .Where(c => c.SetId == setid && c.Id == submitid)
                .BatchUpdateAsync(c => new Submission<Guid>
                {
                    TokenProduced = null,
                    Error = null,
                    Tokens = null
                });

            await Sets
                .Where(s => s.Id == setid)
                .BatchUpdateAsync(s => new PlagiarismSet<Guid>
                {
                    SubmissionSucceeded = s.SubmissionSucceeded - a,
                    SubmissionFailed = s.SubmissionFailed - b,
                });

            await _signalProvider.SendCompileSignalAsync();
        }

        public override async Task<LanguageInfo> FindLanguageAsync(string langName)
        {
            if (_languageProvider != null)
            {
                return await _languageProvider.FindLanguageAsync(langName);
            }
            else
            {
                List<LanguageInfo> langs = await ListLanguageAsync();
                return langs.FirstOrDefault(lang => lang.ShortName == langName);
            }
        }

        public override async Task<List<LanguageInfo>> ListLanguageAsync()
        {
            if (_languageProvider != null)
            {
                return await _languageProvider.ListLanguageAsync();
            }
            else
            {
                string conf = await _configurationRegistry.GetStringAsync(LanguageListConfigurationName);
                return conf.AsJson<List<LanguageInfo>>();
            }
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
                .OrderBy(s => s.Id, asc, true)
                .SkipIf(skip).TakeIf(limit)
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
            var extId = _guidGenerator.Create();

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

            await _signalProvider.SendCompileSignalAsync();
            return e.Entity;
        }

        private class HoistedComparison : Comparison
        {
            [JsonIgnore]
            public Guid ExternalId { get; set; }

            [JsonIgnore]
            public bool? JustificationV2 { get; set; }

            [JsonIgnore]
            public bool? FinishedV2 { get; set; }

            public override string Id
            {
                get => ExternalId.ToString();
                set { }
            }

            public override ReportJustification Justification
            {
                get => Report<Guid>.GetJustificationName(JustificationV2);
                set { }
            }

            public override ReportState State
            {
                get => Report<Guid>.GetProvisioningStateName(FinishedV2);
                set { }
            }
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
                    FinishedV2 = r.Finished,
                    TokensMatched = r.TokensMatched,
                    Percent = r.Percent,
                    PercentIt = r.PercentA,
                    PercentSelf = r.PercentB,
                    ExternalId = r.ExternalId,
                    ExclusiveCategory = s.ExclusiveCategory,
                    JustificationV2 = r.Justification,
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
                    FinishedV2 = r.Finished,
                    TokensMatched = r.TokensMatched,
                    Percent = r.Percent,
                    PercentIt = r.PercentB,
                    PercentSelf = r.PercentA,
                    ExternalId = r.ExternalId,
                    ExclusiveCategory = s.ExclusiveCategory,
                    JustificationV2 = r.Justification,
                };

            return await reportA.Concat(reportB).ToListAsync();
        }

        public override async Task CompileAsync(Guid setid, int submitId, Submission ss, string error, byte[] result)
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
            return s.ToModel(await GetFilesAsync(s.ExternalId));
        }

        public override async Task<int> ScheduleAsync(Guid setId, int submitId, int exclusive, int inclusive, string langId)
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
                insertExpression: s => new Report<Guid> { Finished = null, Justification = null, SetId = s.S, SubmissionA = s.A, SubmissionB = s.B, ExternalId = Guid.NewGuid() },
                updateExpression: (_, __) => new Report<Guid> { Finished = null, Justification = null });

            await Sets
                .Where(c => c.Id == setId)
                .BatchUpdateAsync(c => new PlagiarismSet<Guid>
                {
                    ReportCount = c.ReportCount + affected,
                    ReportPending = c.ReportPending + affected,
                });

            return affected;
        }

        public override async Task<List<ReportTask>> DequeueReportsBatchAsync(int batchSize = 20)
        {
            List<ReportTask> reportTasks = new();
            string sessionKey = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            int retry = 0;
            while (reportTasks.Count == 0 && retry <= 2)
            {
                retry++;

                var reports = await Reports.AsNoTracking()
                    .Where(r => r.Finished == null)
                    .Select(r => new { r.ExternalId, r.SetId, r.SubmissionA, r.SubmissionB })
                    .Take(batchSize)
                    .ToListAsync();

                if (reports.Count == 0) return reportTasks;

                List<Guid> extIds = reports.Select(s => s.ExternalId).ToList();
                await Reports
                    .Where(o => extIds.Contains(o.ExternalId) && o.Finished == null)
                    .BatchUpdateAsync(r => new() { Finished = false, SessionKey = sessionKey });

                reports = await Reports.AsNoTracking()
                    .Where(r => r.Finished == false && r.SessionKey == sessionKey)
                    .Select(r => new { r.ExternalId, r.SetId, r.SubmissionA, r.SubmissionB })
                    .ToListAsync();

                reportTasks.AddRange(reports.Select(r => ReportTask<Guid>.Of(r.ExternalId, r.SetId, r.SubmissionA, r.SubmissionB)));
            }

            return reportTasks;
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

            return ReportTask<Guid>.Of(r.ExternalId, r.SetId, r.SubmissionA, r.SubmissionB);
        }

        public override async Task<IReadOnlyList<SubmissionFile>> GetFilesAsync(Guid extId)
        {
            return await Files.AsNoTracking()
                .Where(s => s.SubmissionId == extId)
                .OrderBy(s => s.FileId)
                .ToListAsync();
        }

        public override async Task SaveReportAsync(ReportTask<Guid> task, ReportFragment fragment)
        {
            await Reports
                .Where(r => r.SetId == task.SetId && r.SubmissionA == task.SubmissionA && r.SubmissionB == task.SubmissionB)
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
                .Where(c => c.Id == task.SetId)
                .BatchUpdateAsync(c => new PlagiarismSet<Guid> { ReportPending = c.ReportPending - 1 });

            await Submissions
                .Where(c => c.SetId == task.SetId && (c.Id == task.SubmissionA || c.Id == task.SubmissionB) && c.MaxPercent < fragment.Percent)
                .BatchUpdateAsync(c => new Submission<Guid> { MaxPercent = fragment.Percent });
        }

        public override async Task SaveReportsAsync(
            List<KeyValuePair<ReportTask<Guid>, ReportFragment>> reports)
        {
            var flatten = reports
                .Select(rf => new
                {
                    rf.Key.SetId,
                    rf.Key.SubmissionA,
                    rf.Key.SubmissionB,
                    rf.Value.TokensMatched,
                    rf.Value.BiggestMatch,
                    rf.Value.Percent,
                    rf.Value.PercentA,
                    rf.Value.PercentB,
                    rf.Value.Matches,
                })
                .ToList();

            await Reports.BatchUpdateJoinAsync(
                inner: flatten,
                outerKeySelector: r => new { r.SetId, r.SubmissionA, r.SubmissionB },
                innerKeySelector: r => new { r.SetId, r.SubmissionA, r.SubmissionB },
                updateSelector: (r, fragment) => new()
                {
                    TokensMatched = fragment.TokensMatched,
                    BiggestMatch = fragment.BiggestMatch,
                    Finished = true,
                    Percent = fragment.Percent,
                    PercentA = fragment.PercentA,
                    PercentB = fragment.PercentB,
                    Matches = fragment.Matches,
                    SessionKey = null,
                });

            var flattenV1 = flatten
                .GroupBy(a => a.SetId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToList();

            await Sets.BatchUpdateJoinAsync(
                inner: flattenV1,
                outerKeySelector: c => c.Id,
                innerKeySelector: c => c.Key,
                updateSelector: (s, c) => new() { ReportPending = s.ReportPending - c.Count });

            var flattenV2 =
                Enumerable.Concat(
                    flatten.Select(a => new { a.SetId, Id = a.SubmissionA, a.Percent }),
                    flatten.Select(a => new { a.SetId, Id = a.SubmissionB, a.Percent }))
                .GroupBy(a => new { a.SetId, a.Id })
                .Select(g => new { g.Key.SetId, g.Key.Id, MaxPercent = g.Max(a => a.Percent) })
                .ToList();

            await Submissions.BatchUpdateJoinAsync(
                inner: flattenV2,
                outerKeySelector: s => new { s.SetId, s.Id },
                innerKeySelector: s => new { s.SetId, s.Id },
                updateSelector: (s, c) => new() { MaxPercent = c.MaxPercent },
                condition: (s, c) => s.MaxPercent < c.MaxPercent);
        }

        public override async Task RefreshCacheAsync()
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
        }

        public override async Task RescueAsync()
        {
            await RefreshCacheAsync();

            await Reports
                .Where(r => r.Finished == false)
                .BatchUpdateAsync(r => new() { Finished = null, SessionKey = null });

            await _signalProvider.SendRescueSignalAsync();
        }

        public override ServiceVersion GetVersion()
        {
            return new ServiceVersion
            {
                FrontendVersion = _languageProvider?.FrontendVersion,
                BackendVersion = typeof(Backend.IBackendRoleStrategy).Assembly.GetName().Version.ToString(),
                Role = "relational_storage"
            };
        }

        public override async Task JustificateAsync(Guid reportid, ReportJustification status)
        {
            bool? internalStatus = status switch
            {
                ReportJustification.Unspecified => default(bool?),
                ReportJustification.Claimed => true,
                ReportJustification.Ignored => false,
                _ => throw new InvalidCastException(),
            };

            var aff = await Reports
                .Where(r => r.ExternalId == reportid)
                .BatchUpdateAsync(r => new Report<Guid> { Justification = internalStatus });

            if (aff == 0)
                throw new KeyNotFoundException("The report doesn't exists.");

            var subs = await Reports
                .Where(r => r.ExternalId == reportid)
                .Select(a => new { a.SetId, a.SubmissionA, a.SubmissionB })
                .SingleAsync();

            await Submissions
                .Where(s => s.SetId == subs.SetId && (s.Id == subs.SubmissionA || s.Id == subs.SubmissionB))
                .BatchUpdateAsync(s => new Submission<Guid>
                {
                    MaxPercent = Reports
                        .Where(r => r.SetId == s.SetId)
                        .Where(r => r.SubmissionA == s.Id || r.SubmissionB == s.Id)
                        .Where(r => r.Justification != false)
                        .Select(r => (double?)r.Percent)
                        .Max() ?? 0
                });
        }

        public override async Task ShareReportAsync(Guid reportid, bool shared)
        {
            var aff = await Reports
                .Where(r => r.ExternalId == reportid)
                .BatchUpdateAsync(r => new Report<Guid> { Shared = shared });

            if (aff == 0)
                throw new KeyNotFoundException("The report doesn't exists.");
        }

        public override async Task<List<KeyValuePair<Submission, Compilation>?>> GetSubmissionsAsync(List<Guid> submitExternalIds)
        {
            var files = await Files
                .Where(s => submitExternalIds.Contains(s.SubmissionId))
                .ToLookupAsync(k => k.SubmissionId, v => v);

            var subs = await Submissions
                .Where(s => submitExternalIds.Contains(s.ExternalId))
                .ToDictionaryAsync(k => k.ExternalId);

            return submitExternalIds
                .Select(s => subs.TryGetValue(s, out var sub)
                    ? default(KeyValuePair<Submission, Compilation>?)
                    : new(sub.ToModel(files[s].ToList()), new() { Error = sub.Error, Tokens = sub.Tokens }))
                .ToList();
        }

        public override async Task<List<KeyValuePair<Submission, Compilation>>> GetSubmissionsAsync(List<(Guid, int)> submitIds)
        {
            Dictionary<Guid, Submission<Guid>> subs = new();
            foreach (var set in submitIds.GroupBy(g => g.Item1))
            {
                var setId = set.Key;
                var subIds = set.Select(a => a.Item2);
                var subss = await Submissions
                    .Where(s => s.SetId == setId && subIds.Contains(s.Id))
                    .ToListAsync();

                subss.ForEach(s => subs.Add(s.ExternalId, s));
            }

            var submitExternalIds = subs.Values.Select(a => a.ExternalId).ToList();
            var files = await Files
                .Where(s => submitExternalIds.Contains(s.SubmissionId))
                .ToLookupAsync(k => k.SubmissionId, v => v);

            return subs.Values.Select(sub =>
                new KeyValuePair<Submission, Compilation>(
                    sub.ToModel(files[sub.ExternalId].ToList()),
                    new() { Error = sub.Error, Tokens = sub.Tokens }))
                .ToList();
        }

        public override Task MigrateAsync()
        {
            return Task.CompletedTask;
        }

        public override Task UpdateLanguagesAsync(List<LanguageInfo> languageSeeds)
        {
            if (_languageProvider != null)
            {
                return _languageProvider.UpdateLanguagesAsync(languageSeeds);
            }
            else
            {
                return _configurationRegistry.UpdateAsync(
                    LanguageListConfigurationName,
                    languageSeeds.ToJson().ToJson());
            }
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
