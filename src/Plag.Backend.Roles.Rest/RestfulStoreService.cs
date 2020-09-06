using Plag.Backend.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Plag.Backend.Services
{
    public class RestfulStoreService : IStoreService
    {
        public Task<PlagiarismSet> CreateSetAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<LanguageInfo> FindLanguageAsync(string langName)
        {
            throw new NotImplementedException();
        }

        public Task<Report> FindReportAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<PlagiarismSet> FindSetAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<Submission> FindSubmissionAsync(string id, bool includeFiles = true)
        {
            throw new NotImplementedException();
        }

        public Task<List<Comparison>> GetComparisonsBySubmissionAsync(string submitId)
        {
            throw new NotImplementedException();
        }

        public Task<Compilation> GetCompilationAsync(string submitId)
        {
            throw new NotImplementedException();
        }

        public Task<List<LanguageInfo>> ListLanguageAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PagedViewList<PlagiarismSet>> ListSetsAsync(int page)
        {
            throw new NotImplementedException();
        }

        public Task<List<Submission>> ListSubmissionsAsync(string setId)
        {
            throw new NotImplementedException();
        }

        public Task SubmitAsync(Submission submission)
        {
            throw new NotImplementedException();
        }
    }
}
