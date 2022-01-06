using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using Plag.Backend.Models;
using Plag.Backend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plag.Backend
{
    internal class CosmosStoreService : IPlagiarismDetectService, IJobContext
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

        public Task<LanguageInfo> FindLanguageAsync(string langName)
        {
            return _database.Languages.GetEntityAsync<LanguageInfo>(langName, langName);
        }

        public async Task<Report> FindReportAsync(string id)
        {
            return !ReportGuid.TryParse(id, out var reportId)
                ? null
                : await _database.Reports.GetEntityAsync<Report>(reportId.ToString(), reportId.GetSetId().ToString());
        }

        public async Task<PlagiarismSet> FindSetAsync(string id)
        {
            return !SetGuid.TryParse(id, out var setGuid)
                ? null
                : await _database.Sets.GetEntityAsync<PlagiarismSet>(setGuid.ToString(), setGuid.ToString());
        }

        public async Task<Submission> FindSubmissionAsync(string setid, int submitid, bool includeFiles = true)
        {
            if (!SetGuid.TryParse(setid, out var setGuid) || !submitid.CanBeInt24())
            {
                return null;
            }

            SubmissionGuid subGuid = SubmissionGuid.FromStructured(setGuid, submitid);
            if (!includeFiles) throw new NotImplementedException();
            return await _database.Submissions.GetEntityAsync<Submission>(subGuid.ToString(), setGuid.ToString());
        }

        public Task<Vertex> GetComparisonsBySubmissionAsync(string setid, int submitid, bool includeFiles = false)
        {
            throw new NotImplementedException();
        }

        public Task<Compilation> GetCompilationAsync(string setid, int submitid)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<SubmissionFile>> GetFilesAsync(string setId, int submitId)
        {
            throw new NotImplementedException();
        }

        public ServiceVersion GetVersion()
        {
            return new ServiceVersion
            {
                BackendVersion = typeof(Backend.IBackendRoleStrategy).Assembly.GetName().Version.ToString(),
                Role = "document_storage"
            };
        }

        public Task<List<LanguageInfo>> ListLanguageAsync()
        {
            return _database.Languages.GetListAsync<LanguageInfo>("SELECT * FROM Languages l");
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

                JObject jobj = await _database.Submissions.SingleOrDefaultAsync<JObject>(
                    "SELECT s.id FROM Submissions s WHERE s.id = @sid",
                    new { sid = subGuid.ToString() },
                    new PartitionKey(set.Id));

                if (jobj != null)
                {
                    throw new ArgumentOutOfRangeException(nameof(set), "Duplicate submission ID.");
                }
            }
            else
            {
                bool upwards = !set.ContestId.HasValue;

                JObject jobj = await _database.Submissions.SingleOrDefaultAsync<JObject>(
                    "SELECT " + (upwards ? "MAX" : "MIN") + "(s.submitid) AS id FROM Submissions s",
                    new PartitionKey(set.Id));

                int? aggId = jobj.TryGetValue("id", out JToken token) && token.Type == JTokenType.Integer
                    ? (int)token
                    : default(int?);

                submitId = upwards
                    ? Math.Max(1, 1 + aggId ?? 0) // maxid < 0 ? fix to 1
                    : Math.Min(-1, -1 + aggId ?? 0); // minid > 0 ? fix to -1

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
                .Patch(set.Id, new PartitionKey(set.Id))
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

            return await _database.Sets.GetListAsync<PlagiarismSet>(query, param);
        }

        public Task JustificateAsync(string reportid, bool? status)
        {
            throw new NotImplementedException();
        }

        public Task RescueAsync()
        {
            throw new NotImplementedException();
        }

        public Task ToggleReportSharenessAsync(string reportid)
        {
            throw new NotImplementedException();
        }

        public async Task ResetCompilationAsync(string setid, int submitid)
        {
            if (!SetGuid.TryParse(setid, out var setGuid) || !submitid.CanBeInt24())
                throw new KeyNotFoundException();
            SubmissionGuid subGuid = SubmissionGuid.FromStructured(setGuid, submitid);

            JObject jobj = await _database.Submissions.SingleOrDefaultAsync<JObject>(
                "SELECT c.token_produced FROM c WHERE c.id = @sid",
                new { sid = subGuid.ToString() },
                new PartitionKey(setGuid.ToString()));

            bool? tokenProduced = (bool?)jobj["token_produced"];
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
                .Patch(setGuid.ToString(), new PartitionKey(setGuid.ToString()))
                .IncrementProperty(s => s.SubmissionSucceeded, a)
                .IncrementProperty(s => s.SubmissionFailed, b)
                .ExecuteAsync();
        }

        public Task<Submission> DequeueSubmissionAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ReportTask> DequeueReportAsync()
        {
            throw new NotImplementedException();
        }

        public Task ScheduleAsync(string setId, int submitId, int exclusive, int inclusive, string langId)
        {
            throw new NotImplementedException();
        }

        public Task SaveReportAsync(ReportTask task, ReportFragment fragment)
        {
            throw new NotImplementedException();
        }
    }
}
