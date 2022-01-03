using Microsoft.Azure.Cosmos;
using Plag.Backend.Entities;
using Plag.Backend.Models;
using Plag.Backend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plag.Backend
{
    internal class CosmosStoreService : IPlagiarismDetectService
    {
        private readonly Database _database;
        private readonly Container _sets;
        private readonly Container _submissions;
        private readonly Container _reports;
        private readonly Container _languages;

        public CosmosStoreService(IPdsCosmosConnection connection)
        {
            _database = connection.GetDatabase();
            _sets = _database.GetContainer(connection.SetsContainerName);
            _submissions = _database.GetContainer(connection.SubmissionsContainerName);
            _reports = _database.GetContainer(connection.ReportsContainerName);
            _languages = _database.GetContainer(connection.LanguagesContainerName);
        }

        public async Task<PlagiarismSet> CreateSetAsync(SetCreation metadata)
        {
            return await _sets.CreateItemAsync<PlagiarismSet>(new()
            {
                Name = metadata.Name,
                ContestId = metadata.ContestId,
                UserId = metadata.UserId,
                CreateTime = DateTimeOffset.Now,
                Id = Guid.NewGuid().ToString(),
            });
        }

        public Task<LanguageInfo> FindLanguageAsync(string langName)
        {
            return _languages.GetEntityAsync<LanguageInfo>(langName, langName);
        }

        public async Task<Report> FindReportAsync(string id)
        {
            List<ReportEntity> reports = await _reports.GetListAsync<ReportEntity>(
                "SELECT * FROM Report c WHERE c.externalid = @id",
                new { id });

            return reports.SingleOrDefault();
        }

        public Task<PlagiarismSet> FindSetAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<Submission> FindSubmissionAsync(string setid, int submitid, bool includeFiles = true)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public Task<Submission> SubmitAsync(SubmissionCreation submission)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Submission>> ListSubmissionsAsync(string setid, string language = null, int? exclusive_category = null, int? inclusive_category = null, double? min_percent = null, int? skip = null, int? limit = null, string order = "id", bool asc = true)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<PlagiarismSet>> ListSetsAsync(int? cid = null, int? uid = null, int? skip = null, int? limit = null, bool asc = false)
        {
            throw new NotImplementedException();
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
    }
}
