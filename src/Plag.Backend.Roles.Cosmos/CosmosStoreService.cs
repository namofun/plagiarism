using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using Plag.Backend.Entities;
using Plag.Backend.Models;
using Plag.Backend.QueryProvider;
using Plag.Backend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Plag.Backend
{
    public class CosmosStoreService : IPlagiarismDetectService, IJobContext
    {
        private readonly ICosmosConnection _database;
        private readonly ILanguageProvider _languageProvider;
        private readonly ISignalProvider _signalProvider;

        public CosmosStoreService(
            ICosmosConnection connection,
            IEnumerable<ILanguageProvider> languageProviders,
            ISignalProvider signalProvider)
        {
            _database = connection;
            _languageProvider = languageProviders.SingleOrDefault();
            _signalProvider = signalProvider;
        }

        public async Task<PlagiarismSet> CreateSetAsync(SetCreation metadata)
        {
            SetEntity entity = await _database.Sets.CreateAsync(new()
            {
                Name = metadata.Name,
                ContestId = metadata.ContestId,
                UserId = metadata.UserId,
                CreateTime = DateTimeOffset.Now,
                Id = SetGuid.New().ToString(),
            });

            await _database.Metadata.CreateAsync(new ServiceGraphEntity { Id = entity.Id });
            return entity;
        }

        public async Task<Report> FindReportAsync(string id)
        {
            return !ReportGuid.TryParse(id, out var reportId)
                ? null
                : await _database.Reports.GetEntityAsync<Report>(
                    reportId.ToString(),
                    reportId.GetSetId().ToString());
        }

        public async Task<PlagiarismSet> FindSetAsync(string id)
        {
            return !SetGuid.TryParse(id, out var setGuid)
                ? null
                : await _database.Sets.GetEntityAsync<PlagiarismSet>(
                    setGuid.ToString(),
                    MetadataEntity.SetsTypeKey);
        }

        private Task<TSubmission> GetSubmissionCoreAsync<TSubmission>(SetGuid setGuid, int submitId, bool includeFiles) where TSubmission : Submission
        {
            return _database.Submissions.SingleOrDefaultAsync<TSubmission>(
                "SELECT s.id, s.setid, s.submitid, s.name, " +
                      " s.exclusive_category, s.inclusive_category, " +
                      " s.max_percent, s.token_produced, s.upload_time, s.language " +
                      (includeFiles ? ", s.files" : "") +
                " FROM Submissions s WHERE s.id = @id AND s.type = \"submission\"",
                new { id = SubmissionGuid.FromStructured(setGuid, submitId).ToString() },
                new PartitionKey(setGuid.ToString()));
        }

        public Task<Submission> FindSubmissionAsync(string setid, int submitid, bool includeFiles = true)
        {
            if (!SetGuid.TryParse(setid, out var setGuid) || !submitid.CanBeInt24())
                return Task.FromResult<Submission>(null);

            return GetSubmissionCoreAsync<Submission>(setGuid, submitid, includeFiles);
        }

        public async Task<Vertex> GetComparisonsBySubmissionAsync(string setid, int submitid, bool includeFiles = false)
        {
            if (!SetGuid.TryParse(setid, out var setGuid) || !submitid.CanBeInt24()) return null;
            Vertex vertex = await GetSubmissionCoreAsync<Vertex>(setGuid, submitid, includeFiles);
            if (vertex == null) return null;

            vertex.Comparisons = await _database.Reports.GetListAsync<Comparison>(
                "SELECT " +
                    " r.id AS reportid, r.state, r.tokens_matched, r.biggest_match, r.percent, r.justification, " +
                    " ((r.submitid_a = @submitid) ? r.submitid_b : r.submitid_a) AS submitid, " +
                    " ((r.submitid_a = @submitid) ? r.submitname_b : r.submitname_a) AS submit, " +
                    " ((r.submitid_a = @submitid) ? r.exclusive_category_b : r.exclusive_category_a) AS exclusive, " +
                    " ((r.submitid_a = @submitid) ? r.percent_a : r.percent_b) AS percent_self, " +
                    " ((r.submitid_a = @submitid) ? r.percent_b : r.percent_a) AS percent_another " +
                " FROM Reports r " +
                " WHERE r.type = \"report\" AND (r.submitid_a = @submitid OR r.submitid_b = @submitid)",
                new { submitid },
                new PartitionKey(setGuid.ToString()));

            return vertex;
        }

        public Task<Compilation> GetCompilationAsync(string setid, int submitid)
        {
            if (!SetGuid.TryParse(setid, out var setGuid) || !submitid.CanBeInt24())
                throw new KeyNotFoundException();

            return _database.Submissions.SingleOrDefaultAsync<Compilation>(
                "SELECT s.error, s.tokens FROM Submissions s WHERE s.id = @id AND s.type = \"submission\"",
                new { id = SubmissionGuid.FromStructured(setGuid, submitid).ToString() },
                new PartitionKey(setGuid.ToString()));
        }

        public async Task<IReadOnlyList<SubmissionFile>> GetFilesAsync(string setId, int submitId)
        {
            if (!SetGuid.TryParse(setId, out var setGuid) || !submitId.CanBeInt24())
                throw new KeyNotFoundException();
            SubmissionGuid subGuid = SubmissionGuid.FromStructured(setGuid, submitId);

            QuickResult<List<SubmissionFile>> result =
                await _database.Submissions.SingleOrDefaultAsync<QuickResult<List<SubmissionFile>>>(
                    "SELECT s.files AS result FROM Submissions s WHERE s.id = @sid",
                    new { sid = subGuid.ToString() },
                    new PartitionKey(setGuid.ToString()));

            return result?.Result;
        }

        public ServiceVersion GetVersion()
        {
            return new ServiceVersion
            {
                FrontendVersion = _languageProvider?.FrontendVersion,
                BackendVersion = typeof(Backend.IBackendRoleStrategy).Assembly.GetName().Version.ToString(),
                Role = "document_storage"
            };
        }

        public async Task<List<LanguageInfo>> ListLanguageAsync()
        {
            if (_languageProvider != null)
            {
                return await _languageProvider.ListLanguageAsync();
            }
            else
            {
                var metadata = await _database.Metadata
                    .GetEntityAsync<MetadataEntity<List<LanguageInfo>>>(
                        MetadataEntity.LanguagesMetadataKey,
                        MetadataEntity.SettingsTypeKey);

                return metadata.Data;
            }
        }

        public Task<LanguageInfo> FindLanguageAsync(string langName)
        {
            if (_languageProvider != null)
            {
                return _languageProvider.FindLanguageAsync(langName);
            }
            else
            {
                return _database.Metadata.SingleOrDefaultAsync<LanguageInfo>(
                    "SELECT * FROM l in Metadata.data WHERE l.id = @id",
                    new { id = langName },
                    new PartitionKey(MetadataEntity.SettingsTypeKey));
            }
        }

        public async Task<Submission> SubmitAsync(SubmissionCreation submission)
        {
            PlagiarismSet set = await FindSetAsync(submission.SetId);
            if (set == null) throw new KeyNotFoundException("Set not found.");

            int submitId;
            SubmissionGuid subGuid;

            if (submission.Id.HasValue)
            {
                submitId = submission.Id.Value;
                subGuid = SubmissionGuid.FromStructured(SetGuid.Parse(set.Id), submitId);

                QuickResult<string> sidCheck =
                    await _database.Submissions.SingleOrDefaultAsync<QuickResult<string>>(
                        "SELECT s.id AS result FROM Submissions s WHERE s.id = @sid",
                        new { sid = subGuid.ToString() },
                        new PartitionKey(set.Id));

                if (sidCheck != null)
                {
                    throw new ArgumentOutOfRangeException(nameof(set), "Duplicate submission ID.");
                }
            }
            else
            {
                bool upwards = !set.ContestId.HasValue;

                QuickResult<int?> agg =
                    await _database.Submissions.SingleOrDefaultAsync<QuickResult<int?>>(
                        "SELECT " + (upwards ? "MAX" : "MIN") + "(s.submitid) AS result FROM Submissions s",
                        new PartitionKey(set.Id));

                submitId = upwards
                    ? Math.Max(1, 1 + agg.Result ?? 0) // maxid < 0 ? fix to 1
                    : Math.Min(-1, -1 + agg.Result ?? 0); // minid > 0 ? fix to -1

                subGuid = SubmissionGuid.FromStructured(SetGuid.Parse(set.Id), submitId);
            }

            Submission s = await _database.Submissions.CreateAsync(new()
            {
                ExternalId = subGuid.ToString(),
                Id = submitId,
                SetId = set.Id,
                ExclusiveCategory = submission.ExclusiveCategory ?? submitId,
                InclusiveCategory = submission.InclusiveCategory,
                Language = submission.Language,
                Name = submission.Name,
                UploadTime = DateTimeOffset.Now,
                Files = submission.Files.Select((i, j) => new SubmissionFile(j + 1, i)).ToList(),
            });

            await _database.Sets
                .Patch(set.Id, new PartitionKey(MetadataEntity.SetsTypeKey))
                .IncrementProperty(s => s.SubmissionCount, 1)
                .ExecuteAsync();

            await _signalProvider.SendCompileSignalAsync();

            s.Files = null;
            return s;
        }

        public async Task<IReadOnlyList<Submission>> ListSubmissionsAsync(
            string setid,
            string language = null,
            int? exclusive_category = null,
            int? inclusive_category = null,
            double? min_percent = null,
            int? skip = null,
            int? limit = null,
            string order = "id",
            bool asc = true)
        {
            if (skip.HasValue != limit.HasValue)
            {
                throw new ArgumentOutOfRangeException(nameof(skip), "Must specify skip and limit at the same time when querying sets.");
            }

            string query =
                "SELECT c.setid, c.submitid, c.externalid, c.upload_time, " +
                      " c.exclusive_category, c.inclusive_category, c.language, " +
                      " c.name, c.max_percent, c.token_produced " +
                "FROM Submissions c WHERE c.type = \"submission\"";
            JObject param = new();

            if (exclusive_category.HasValue)
            {
                query += " AND c.exclusive_category = @exclid";
                param["exclid"] = exclusive_category.Value;
            }

            if (inclusive_category.HasValue)
            {
                query += " AND c.inclusive_category = @inclid";
                param["inclid"] = inclusive_category.Value;
            }

            if (!string.IsNullOrEmpty(language))
            {
                query += " AND c.language = @langid";
                param["langid"] = language;
            }

            if (min_percent.HasValue)
            {
                query += " AND c.max_percent > @percent";
                param["percent"] = min_percent.Value;
            }

            query += " ORDER BY c." + (order == "percent" ? "max_percent" : "id") + " " + (asc ? "ASC" : "DESC");

            if (skip.HasValue)
            {
                query += " OFFSET @skip LIMIT @limit";
                param["skip"] = skip.Value;
                param["limit"] = limit.Value;
            }

            return await _database.Submissions.GetListAsync<Submission>(query, param);
        }

        public async Task<IReadOnlyList<PlagiarismSet>> ListSetsAsync(
            int? cid = null,
            int? uid = null,
            int? skip = null,
            int? limit = null,
            bool asc = false)
        {
            if (skip.HasValue != limit.HasValue)
            {
                throw new ArgumentOutOfRangeException(nameof(skip), "Must specify skip and limit at the same time when querying sets.");
            }

            string query = "SELECT * FROM Sets c WHERE c.type = \"sets\"";
            JObject param = new();

            if (cid.HasValue)
            {
                query += " AND c.related = @cid";
                param["cid"] = cid.Value;
            }

            if (uid.HasValue)
            {
                query += " AND c.creator = @uid";
                param["uid"] = cid.Value;
            }

            query += " ORDER BY c.create_time " + (asc ? "ASC" : "DESC");

            if (skip.HasValue)
            {
                query += " OFFSET @skip LIMIT @limit";
                param["skip"] = skip.Value;
                param["limit"] = limit.Value;
            }

            return await _database.Sets.GetListAsync<PlagiarismSet>(
                query,
                param,
                new PartitionKey(MetadataEntity.SetsTypeKey));
        }

        public async Task ResetCompilationAsync(string setid, int submitid)
        {
            if (!SetGuid.TryParse(setid, out var setGuid) || !submitid.CanBeInt24())
                throw new KeyNotFoundException();
            SubmissionGuid subGuid = SubmissionGuid.FromStructured(setGuid, submitid);

            QuickResult<bool?> jobj =
                await _database.Submissions.SingleOrDefaultAsync<QuickResult<bool?>>(
                    "SELECT c.token_produced AS result FROM c WHERE c.id = @sid AND c.type = \"submission\"",
                    new { sid = subGuid.ToString() },
                    new PartitionKey(setGuid.ToString()));

            bool? tokenProduced = jobj.Result;
            if (!tokenProduced.HasValue) throw new ArgumentOutOfRangeException();

            await _database.Submissions
                .Patch(subGuid.ToString(), new PartitionKey(setGuid.ToString()))
                .SetProperty(s => s.Error, null)
                .SetProperty(s => s.Tokens, null)
                .SetProperty(s => s.TokenProduced, null)
                .ConcurrencyGuard("FROM c WHERE c.token_produced <> null")
                .ExecuteAsync();

            var (a, b) = tokenProduced.Value ? (1, 0) : (0, 1);
            await _database.Sets
                .Patch(setGuid.ToString(), new PartitionKey(MetadataEntity.SetsTypeKey))
                .IncrementProperty(s => s.SubmissionSucceeded, -a)
                .IncrementProperty(s => s.SubmissionFailed, -b)
                .ExecuteAsync();

            await _signalProvider.SendCompileSignalAsync();
        }

        public async Task CompileAsync(Submission submission, string error, byte[] result)
        {
            if (!SetGuid.TryParse(submission.SetId, out var setGuid) || !submission.Id.CanBeInt24())
                throw new KeyNotFoundException();
            SubmissionGuid subGuid = SubmissionGuid.FromStructured(setGuid, submission.Id);

            bool tokenProduced = result != null;
            await _database.Submissions
                .Patch(subGuid.ToString(), new PartitionKey(setGuid.ToString()))
                .SetProperty(s => s.Error, error)
                .SetProperty(s => s.Tokens, result)
                .SetProperty(s => s.TokenProduced, tokenProduced)
                .ConcurrencyGuard("FROM c WHERE c.token_produced = null")
                .ExecuteAsync();

            var (a, b) = tokenProduced ? (1, 0) : (0, 1);
            await _database.Sets
                .Patch(setGuid.ToString(), new PartitionKey(MetadataEntity.SetsTypeKey))
                .IncrementProperty(s => s.SubmissionSucceeded, a)
                .IncrementProperty(s => s.SubmissionFailed, b)
                .ExecuteAsync();

            if (tokenProduced)
            {
                ServiceGraphEntity.Vertex vertex = new()
                {
                    Id = submission.Id,
                    Exclusive = submission.ExclusiveCategory,
                    Inclusive = submission.InclusiveCategory,
                    Language = submission.Language,
                    Name = submission.Name,
                };

                await _database.Metadata.GetContainer().PatchItemAsync<ServiceGraphEntity>(
                    setGuid.ToString(),
                    new PartitionKey(MetadataEntity.ServiceGraphTypeKey),
                    new[] { PatchOperation.Set("/data/" + subGuid.ToString(), vertex) },
                    new() { EnableContentResponseOnWrite = false });
            }
        }

        public Task<Submission> DequeueSubmissionAsync()
        {
            return _database.Submissions.SingleOrDefaultAsync<Submission>(
                "SELECT TOP 1 * FROM Submissions s WHERE s.token_produced = null AND s.type = \"submission\"",
                default(PartitionKey?));
        }

        public async Task<ReportTask> DequeueReportAsync()
        {
            ReportTask reportTask = null;
            int retry = 0;
            while (reportTask == null && retry <= 2)
            {
                QuickResult<string> topReportId =
                    await _database.Reports.SingleOrDefaultAsync<QuickResult<string>>(
                        "SELECT TOP 1 r.id AS result FROM Reports r WHERE r.state = \"Pending\" AND r.type = \"report\"",
                        default(PartitionKey?));

                if (topReportId == null) return null;
                ReportGuid reportGuid = ReportGuid.Parse(topReportId.Result);
                ReportTask tempTask = new(
                    reportGuid.ToString(),
                    reportGuid.GetSetId().ToString(),
                    reportGuid.GetIdOfA(),
                    reportGuid.GetIdOfB());

                try
                {
                    await _database.Reports
                        .Patch(tempTask.Id, new PartitionKey(tempTask.SetId))
                        .SetProperty(r => r.State, ReportState.Analyzing)
                        .ConcurrencyGuard("FROM r WHERE r.state = \"Pending\"")
                        .ExecuteAsync();
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
                {
                    retry++;
                    continue;
                }

                reportTask = tempTask;
            }

            return reportTask;
        }

        public async Task<int> ScheduleAsync(Submission u)
        {
            var set = SetGuid.Parse(u.SetId);
            var vertices = await _database.Metadata.QueryServiceGraphAsync(set, u.Language, u.InclusiveCategory, u.ExclusiveCategory);
            if (vertices.Resource.Count == 0) return 0;

            var pendingReports = vertices.Resource
                .OrderBy(x => x.Id)
                .Select(v => ReportEntity.Of(set, (u.Id, u.Name, u.ExclusiveCategory), (v.Id, v.Name, v.Exclusive)))
                .ToList();

            int created = 0, updated = 0;
            await _database.Reports.BatchWithRetryAsync(set.ToString(), pendingReports, (report, batch) =>
            {
                batch.UpsertItem(report, new() { EnableContentResponseOnWrite = false });
            },
            async (_, reports) =>
            {
                created += reports.Count(a => a.Item2.StatusCode == HttpStatusCode.Created);
                updated += reports.Count(a => a.Item2.StatusCode == HttpStatusCode.OK);

                if (created + updated > 30)
                {
                    await _database.Sets
                        .Patch(set.ToString(), new PartitionKey(MetadataEntity.SetsTypeKey))
                        .IncrementProperty(s => s.ReportCount, created)
                        .IncrementProperty(s => s.ReportPending, created + updated)
                        .ExecuteAsync();

                    created = updated = 0;
                }
            });

            await _database.Sets
                .Patch(set.ToString(), new PartitionKey(MetadataEntity.SetsTypeKey))
                .IncrementProperty(s => s.ReportCount, created)
                .IncrementProperty(s => s.ReportPending, created + updated)
                .ExecuteAsync();

            return pendingReports.Count;
        }

        public async Task RefreshCacheAsync()
        {
            List<JObject> reportAggregate =
                await _database.Reports.GetListAsync<JObject>(
                    "SELECT r.setid, COUNT(1) AS report_count, " +
                          " SUM(r.state = \"Finished\" ? 0 : 1) AS report_pending " +
                    "FROM Reports r WHERE r.type = \"report\" " +
                    "GROUP BY r.setid");

            List<JObject> submissionAggregate =
                await _database.Submissions.GetListAsync<JObject>(
                    "SELECT s.setid, COUNT(1) AS submission_count, " +
                          " SUM(s.token_produced = true ? 1 : 0) AS submission_succeeded, " +
                          " SUM(s.token_produced = false ? 1 : 0) AS submission_failed " +
                    "FROM Submissions s WHERE s.type = \"submission\" " +
                    "GROUP BY s.setid");

            JObject aggProps = JObject.FromObject(new
            {
                report_count = 0,
                report_pending = 0,
                submission_count = 0,
                submission_succeeded = 0,
                submission_failed = 0,
            });

            Dictionary<string, JObject> patchEntries = new();
            foreach (JObject agg in submissionAggregate.Concat(reportAggregate))
            {
                string setId = (string)agg["setid"];
                if (!patchEntries.ContainsKey(setId)) patchEntries.Add(setId, new JObject(aggProps));
                patchEntries[setId].Merge(agg);
            }

            IEnumerable<string> keys = ((IDictionary<string, JToken>)aggProps).Keys;
            await _database.Sets.BatchWithRetryAsync(MetadataEntity.SetsTypeKey, patchEntries, (patchEntry, batch) =>
            {
                batch.PatchItem(
                    patchEntry.Key,
                    keys.Select(aggKey => PatchOperation.Replace($"/{aggKey}", (int)patchEntry.Value[aggKey])).ToList(),
                    new() { EnableContentResponseOnWrite = false });
            });
        }

        public async Task RescueAsync()
        {
            await RefreshCacheAsync();

            List<string> analyzings =
                await _database.Reports.GetListAsync<string>(
                    "SELECT VALUE r.id FROM Reports r " +
                    "WHERE r.state = \"Analyzing\" AND r.type = \"report\"");

            await _database.Reports.BatchWithRetryAsync(
                analyzings.Select(a => ReportGuid.Parse(a)),
                guid => guid.GetSetId().ToString(),
                batchSize: 30,
                batchEntryBuilder: (report, batch) =>
                {
                    batch.PatchItem(
                        report.ToString(),
                        new[] { PatchOperation.Set("/state", ReportState.Pending) },
                        new() { EnableContentResponseOnWrite = false, FilterPredicate = "FROM r WHERE r.state = \"Analyzing\"" });
                });

            await _signalProvider.SendRescueSignalAsync();
        }

        public async Task SaveReportAsync(ReportTask task, ReportFragment fragment)
        {
            ReportGuid reportGuid = ReportGuid.Parse(task.Id);
            SetGuid setGuid = reportGuid.GetSetId();
            if (task.SetId != setGuid.ToString()
                || task.SubmissionA != reportGuid.GetIdOfA()
                || task.SubmissionB != reportGuid.GetIdOfB())
                throw new InvalidOperationException("Unknown report.");

            await _database.Reports
                .Patch(reportGuid.ToString(), new PartitionKey(task.SetId))
                .SetProperty(r => r.TokensMatched, fragment.TokensMatched)
                .SetProperty(r => r.BiggestMatch, fragment.BiggestMatch)
                .SetProperty(r => r.State, ReportState.Finished)
                .SetProperty(r => r.Percent, fragment.Percent)
                .SetProperty(r => r.PercentA, fragment.PercentA)
                .SetProperty(r => r.PercentB, fragment.PercentB)
                .SetProperty(r => r.Matches, fragment.Matches)
                .ExecuteAsync();

            await _database.Sets
                .Patch(task.SetId, new PartitionKey(MetadataEntity.SetsTypeKey))
                .IncrementProperty(s => s.ReportPending, -1)
                .ExecuteAsync();

            await _database.Submissions.BatchWithRetryAsync(task.SetId, new[] { task.SubmissionA, task.SubmissionB }, (sid, batch) =>
            {
                batch.PatchItem(
                    SubmissionGuid.FromStructured(setGuid, sid).ToString(),
                    new[] { PatchOperation.Set("/max_percent", fragment.Percent) },
                    new() { FilterPredicate = "FROM s WHERE s.max_percent < " + fragment.Percent.ToJson(), EnableContentResponseOnWrite = false });
            });
        }

        public async Task JustificateAsync(string reportid, ReportJustification status)
        {
            ReportGuid reportGuid = ReportGuid.Parse(reportid);
            SetGuid setGuid = reportGuid.GetSetId();

            await _database.Reports
                .Patch(reportGuid.ToString(), new PartitionKey(setGuid.ToString()))
                .SetProperty(r => r.Justification, status)
                .ExecuteAsync();

            foreach (int id in new[] { reportGuid.GetIdOfA(), reportGuid.GetIdOfB() })
            {
                QuickResult<double?> agg =
                    await _database.Reports.SingleOrDefaultAsync<QuickResult<double?>>(
                        "SELECT MAX(r.percent) AS result FROM Reports r WHERE (r.submitid_a = @id OR r.submitid_b = @id) AND r.justification <> \"Ignored\" AND r.type = \"report\"",
                        new { id },
                        new PartitionKey(setGuid.ToString()));

                await _database.Submissions
                    .Patch(SubmissionGuid.FromStructured(setGuid, id).ToString(), new PartitionKey(setGuid.ToString()))
                    .SetProperty(s => s.MaxPercent, agg.Result ?? 0)
                    .ExecuteAsync();
            }
        }

        public Task ShareReportAsync(string reportid, bool shared)
        {
            ReportGuid reportGuid = ReportGuid.Parse(reportid);

            return _database.Reports
                .Patch(reportGuid.ToString(), new PartitionKey(reportGuid.GetSetId().ToString()))
                .SetProperty(r => r.Shared, shared)
                .ExecuteAsync();
        }

        public async Task<List<ReportTask>> DequeueReportsBatchAsync(int batchSize = 20)
        {
            List<ReportTask> reportTasks = new();
            string sessionKey = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            int retry = 0;
            while (reportTasks.Count == 0 && retry <= 2)
            {
                List<string> topReportIds =
                    await _database.Reports.GetListAsync<string>(
                        "SELECT TOP " + batchSize + " VALUE r.id FROM Reports r " +
                        "WHERE r.state = \"Pending\" AND r.type = \"report\"");

                if (topReportIds.Count == 0) return reportTasks;
                await _database.Reports.BatchAsync(
                    topReportIds.Select(id => ReportGuid.Parse(id)),
                    r => r.GetSetId().ToString(),
                    batchSize: batchSize,
                    transactional: false,
                    allowTooManyRequests: true,
                    batchEntryBuilder: (task, batch) =>
                    {
                        batch.PatchItem(
                            task.ToString(),
                            new[] { PatchOperation.Set("/state", ReportState.Analyzing), PatchOperation.Set("/session_lock", sessionKey) },
                            new() { FilterPredicate = "FROM r WHERE r.state = \"Pending\"", EnableContentResponseOnWrite = false });
                    },
                    postBatchResponse: (setId, tasks, resp) =>
                    {
                        if (tasks.Length != resp.Count)
                        {
                            throw new InvalidOperationException("Unknown mismatch scenario.");
                        }

                        foreach ((ReportGuid report, var result) in tasks.Zip(resp))
                        {
                            if (result.IsSuccessStatusCode)
                            {
                                reportTasks.Add(new(report.ToString(), setId, report.GetIdOfA(), report.GetIdOfB()));
                            }
                        }
                    });
            }

            return reportTasks;
        }

        public async Task SaveReportsAsync(List<KeyValuePair<ReportTask, ReportFragment>> reports)
        {
            static string P<T>(System.Linq.Expressions.Expression<Func<ReportEntity, T>> expression)
                => "/" + expression.ParseProperty();

            await _database.Reports.BatchWithRetryAsync(reports, r => r.Key.SetId, (report, batch) =>
            {
                (ReportTask task, ReportFragment fragment) = report;
                batch.PatchItem(task.Id, new[]
                {
                    PatchOperation.Set(P(r => r.TokensMatched), fragment.TokensMatched),
                    PatchOperation.Set(P(r => r.BiggestMatch), fragment.BiggestMatch),
                    PatchOperation.Set(P(r => r.State), ReportState.Finished),
                    PatchOperation.Set(P(r => r.Percent), fragment.Percent),
                    PatchOperation.Set(P(r => r.PercentA), fragment.PercentA),
                    PatchOperation.Set(P(r => r.PercentB), fragment.PercentB),
                    PatchOperation.Set(P(r => r.Matches), fragment.Matches),
                },
                new() { EnableContentResponseOnWrite = false });
            });

            var setAgg = reports.GroupBy(a => a.Key.SetId).Select(a => new { a.Key, Count = a.Count() });
            await _database.Sets.BatchWithRetryAsync(MetadataEntity.SetsTypeKey, setAgg, (model, batch) =>
            {
                batch.PatchItem(model.Key, new[]
                {
                    PatchOperation.Increment("/report_pending", -model.Count)
                },
                new() { EnableContentResponseOnWrite = false });
            });

            IEnumerable<(string, string, double)> submitPercent =
                Enumerable.Concat(
                    reports.Select(a => new { a.Key.SetId, SubmitId = a.Key.SubmissionA, a.Value.Percent }),
                    reports.Select(a => new { a.Key.SetId, SubmitId = a.Key.SubmissionB, a.Value.Percent }))
                .GroupBy(a => SubmissionGuid.FromStructured(SetGuid.Parse(a.SetId), a.SubmitId))
                .Select(a => (a.Key.GetSetId().ToString(), a.Key.ToString(), a.Select(b => b.Percent).Max()));

            await _database.Submissions.BatchWithRetryAsync(submitPercent, a => a.Item1, (submission, batch) =>
            {
                batch.PatchItem(
                    submission.Item2,
                    new[] { PatchOperation.Set("/max_percent", submission.Item3) },
                    new() { FilterPredicate = "FROM s WHERE s.max_percent < " + submission.Item3.ToJson(), EnableContentResponseOnWrite = false });
            });
        }

        public async Task<List<KeyValuePair<Submission, Compilation>?>> GetSubmissionsAsync(List<string> submitExternalIds)
        {
            List<(string id, PartitionKey partitionKey)> submissionKeys = new();
            foreach (string submitExternalId in submitExternalIds)
            {
                if (!SubmissionGuid.TryParse(submitExternalId, out var subGuid))
                {
                    throw new Exception("Unable to parse the submission ID.");
                }

                submissionKeys.Add((subGuid.ToString(), new(subGuid.GetSetId().ToString())));
            }

            Dictionary<string, SubmissionEntity> results =
                (await _database.Submissions.GetContainer()
                    .ReadManyItemsAsync<SubmissionEntity>(submissionKeys))
                .ToDictionary(k => k.ExternalId);

            return submitExternalIds
                .Select(s => results.TryGetValue(s, out var sub)
                    ? default(KeyValuePair<Submission, Compilation>?)
                    : new(sub, new() { Error = sub.Error, Tokens = sub.Tokens }))
                .ToList();
        }

        public async Task<List<KeyValuePair<Submission, Compilation>>> GetSubmissionsAsync(List<(string, int)> submitIds)
        {
            List<(string id, PartitionKey partitionKey)> submissionKeys = new();
            foreach ((string setId, int subId) in submitIds)
            {
                if (!SetGuid.TryParse(setId, out var setGuid))
                {
                    continue;
                }

                SubmissionGuid subGuid = SubmissionGuid.FromStructured(setGuid, subId);
                submissionKeys.Add((subGuid.ToString(), new(setId.ToString())));
            }

            FeedResponse<SubmissionEntity> results =
                await _database.Submissions.GetContainer()
                    .ReadManyItemsAsync<SubmissionEntity>(submissionKeys);

            return results.Select(sub =>
                new KeyValuePair<Submission, Compilation>(
                    sub,
                    new() { Error = sub.Error, Tokens = sub.Tokens }))
                .ToList();
        }

        public Task MigrateAsync()
        {
            return _database.MigrateAsync();
        }

        public Task UpdateLanguagesAsync(List<LanguageInfo> languageSeeds)
        {
            if (_languageProvider != null)
            {
                return _languageProvider.UpdateLanguagesAsync(languageSeeds);
            }
            else
            {
                return _database.Metadata.UpsertAsync(new MetadataEntity<List<LanguageInfo>>()
                {
                    Id = MetadataEntity.LanguagesMetadataKey,
                    Type = MetadataEntity.SettingsTypeKey,
                    Data = languageSeeds,
                });
            }
        }
    }
}
