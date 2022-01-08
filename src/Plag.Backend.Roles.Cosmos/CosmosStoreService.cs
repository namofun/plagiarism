using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using Plag.Backend.Entities;
using Plag.Backend.Models;
using Plag.Backend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plag.Backend
{
    public class CosmosStoreService : IPlagiarismDetectService, IJobContext
    {
        private readonly ICosmosConnection _database;

        public CosmosStoreService(ICosmosConnection connection)
        {
            _database = connection;
        }

        public async Task<PlagiarismSet> CreateSetAsync(SetCreation metadata)
        {
            return await _database.Sets.CreateAsync(new()
            {
                Name = metadata.Name,
                ContestId = metadata.ContestId,
                UserId = metadata.UserId,
                CreateTime = DateTimeOffset.Now,
                Id = SetGuid.New().ToString(),
            });
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
                " FROM Submissions s WHERE s.id = @id",
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
                " WHERE r.submitid_a = @submitid OR r.submitid_b = @submitid",
                new { submitid },
                new PartitionKey(setGuid.ToString()));

            return vertex;
        }

        public Task<Compilation> GetCompilationAsync(string setid, int submitid)
        {
            if (!SetGuid.TryParse(setid, out var setGuid) || !submitid.CanBeInt24())
                throw new KeyNotFoundException();

            return _database.Submissions.SingleOrDefaultAsync<Compilation>(
                "SELECT s.error, s.tokens FROM Submissions s WHERE s.id = @id",
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
                BackendVersion = typeof(Backend.IBackendRoleStrategy).Assembly.GetName().Version.ToString(),
                Role = "document_storage"
            };
        }

        public async Task<List<LanguageInfo>> ListLanguageAsync()
        {
            var metadata = await _database.Metadata
                .GetEntityAsync<MetadataEntity<List<LanguageInfo>>>(
                    MetadataEntity.LanguagesMetadataKey,
                    MetadataEntity.LanguagesMetadataKey);

            return metadata.Data;
        }

        public Task<LanguageInfo> FindLanguageAsync(string langName)
        {
            return _database.Metadata.SingleOrDefaultAsync<LanguageInfo>(
                "SELECT * FROM l in Metadata.data WHERE l.id = @id",
                new { id = langName },
                new PartitionKey(MetadataEntity.LanguagesMetadataKey));
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

            string query = "SELECT * FROM Submissions c";
            JObject param = new();

            if (exclusive_category.HasValue)
            {
                query += param.Count == 0 ? " WHERE" : " AND";
                query += " c.exclusive_category = @exclid";
                param["exclid"] = exclusive_category.Value;
            }

            if (inclusive_category.HasValue)
            {
                query += param.Count == 0 ? " WHERE" : " AND";
                query += " c.inclusive_category = @inclid";
                param["inclid"] = inclusive_category.Value;
            }

            if (!string.IsNullOrEmpty(language))
            {
                query += param.Count == 0 ? " WHERE" : " AND";
                query += " c.language = @langid";
                param["langid"] = language;
            }

            if (min_percent.HasValue)
            {
                query += param.Count == 0 ? " WHERE" : " AND";
                query += " c.max_percent = @percent";
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

            string query = "SELECT * FROM Sets c";
            JObject param = new();

            if (cid.HasValue)
            {
                query += param.Count == 0 ? " WHERE" : " AND";
                query += " c.related = @cid";
                param["cid"] = cid.Value;
            }

            if (uid.HasValue)
            {
                query += param.Count == 0 ? " WHERE" : " AND";
                query += " c.creator = @uid";
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
                    "SELECT c.token_produced AS result FROM c WHERE c.id = @sid",
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
                .Patch(setGuid.ToString(), new PartitionKey(setGuid.ToString()))
                .IncrementProperty(s => s.SubmissionSucceeded, -a)
                .IncrementProperty(s => s.SubmissionFailed, -b)
                .ExecuteAsync();
        }

        public async Task CompileAsync(string setid, int submitId, string error, byte[] result)
        {
            if (!SetGuid.TryParse(setid, out var setGuid) || !submitId.CanBeInt24())
                throw new KeyNotFoundException();
            SubmissionGuid subGuid = SubmissionGuid.FromStructured(setGuid, submitId);

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
        }

        public Task<Submission> DequeueSubmissionAsync()
        {
            return _database.Submissions.SingleOrDefaultAsync<Submission>(
                "SELECT TOP 1 * FROM Submissions s WHERE s.token_produced = null",
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
                        "SELECT TOP 1 r.id AS result FROM Reports r WHERE r.state = \"Pending\"",
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

        public Task ScheduleAsync(string setId, int submitId, int exclusive, int inclusive, string langId)
        {
            throw new NotImplementedException();
        }

        public async Task RescueAsync()
        {
            List<JObject> reportAggregate =
                await _database.Reports.GetListAsync<JObject>(
                    "SELECT r.setid, COUNT(1) AS report_count, " +
                          " SUM(r.state = \"Finished\" ? 1 : 0) AS report_pending " +
                    "FROM Reports r " +
                    "GROUP BY r.setid");

            List<JObject> submissionAggregate =
                await _database.Submissions.GetListAsync<JObject>(
                    "SELECT s.setid, COUNT(1) AS submission_count, " +
                          " SUM(s.token_produced = true ? 1 : 0) AS submission_succeeded, " +
                          " SUM(s.token_produced = false ? 1 : 0) AS submission_failed " +
                    "FROM Submissions s " +
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

            Container container = _database.Sets.GetContainer();
            IEnumerable<string> keys = ((IDictionary<string, JToken>)aggProps).Keys;
            foreach (JObject[] chunk in patchEntries.Values.Chunk(100))
            {
                TransactionalBatch batch =
                    container.CreateTransactionalBatch(
                        new PartitionKey(MetadataEntity.SetsTypeKey));

                foreach (JObject patchEntry in chunk)
                {
                    batch.PatchItem(
                        (string)patchEntry["setid"],
                        keys.Select(aggKey => PatchOperation.Replace($"/{aggKey}", (int)patchEntry[aggKey])).ToList(),
                        new TransactionalBatchPatchItemRequestOptions() { EnableContentResponseOnWrite = false });
                }

                await batch.ExecuteAsync();
            }

            throw new NotImplementedException();
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

            foreach (int sid in new[] { task.SubmissionA, task.SubmissionB })
            {
                await _database.Submissions
                    .Patch(
                        SubmissionGuid.FromStructured(setGuid, sid).ToString(),
                        new PartitionKey(task.SetId))
                    .SetProperty(s => s.MaxPercent, fragment.Percent)
                    .UpdateOnCondition("FROM s WHERE s.max_percent < " + fragment.Percent.ToJson())
                    .ExecuteAsync();
            }
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
                        "SELECT MAX(r.percent) AS result FROM Reports r WHERE (r.submitid_a = @id OR r.submitid_b = @id) AND r.justification <> \"Ignored\"",
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
    }
}
