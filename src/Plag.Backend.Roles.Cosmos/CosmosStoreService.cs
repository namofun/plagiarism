using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using Plag.Backend.Models;
using Plag.Backend.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plag.Backend
{
    internal class CosmosStoreService : IPlagiarismDetectService
    {
        private readonly ICosmosConnection _database;

        public CosmosStoreService(ICosmosConnection connection)
        {
            _database = connection;
        }

        public async Task<PlagiarismSet> CreateSetAsync(SetCreation metadata)
        {
            return await _database.Sets.GetContainer().CreateItemAsync<PlagiarismSet>(new()
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

        public Task<Submission> SubmitAsync(SubmissionCreation submission)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Submission>> ListSubmissionsAsync(
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
            throw new NotImplementedException();
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
                throw new NotSupportedException("Must specify skip and limit at the same time when querying sets.");
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

        public Task ResetCompilationAsync(string setid, int submitid)
        {
            throw new NotImplementedException();
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
    }
}
