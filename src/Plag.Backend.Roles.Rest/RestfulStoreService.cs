using Plag.Backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plag.Backend.Services
{
    public class RestfulStoreService : IPlagiarismDetectService
    {
        public RestfulClient Client { get; }

        public RestfulStoreService(RestfulClient client)
            => Client = client;

        public Task<LanguageInfo> FindLanguageAsync(string id)
            => Client.GetAsync<LanguageInfo>($"/languages/{id}");

        public Task<Report> FindReportAsync(string id)
            => Client.GetAsync<Report>($"/reports/{id}");

        public Task<PlagiarismSet> FindSetAsync(string id)
            => Client.GetAsync<PlagiarismSet>($"/sets/{id}");

        public Task<Compilation> GetCompilationAsync(string sid, int id)
            => Client.GetAsync<Compilation>($"/sets/{sid}/submissions/{id}/compilation");

        public Task<Submission> FindSubmissionAsync(string sid, int id, bool includeFiles = true)
            => Client.GetAsync<Submission>($"/sets/{sid}/submissions/{id}?includeFiles={includeFiles}");

        public async Task<IReadOnlyList<Comparison>> GetComparisonsBySubmissionAsync(string sid, int id)
            => await Client.GetAsync<List<Comparison>>($"/sets/{sid}/submissions/{id}/comparisons");

        public async Task<IReadOnlyList<SubmissionFile>> GetFilesAsync(string sid, int id)
            => await Client.GetAsync<List<SubmissionFile>>($"/sets/{sid}/submissions/{id}/files");

        public Task<List<LanguageInfo>> ListLanguageAsync()
            => Client.GetAsync<List<LanguageInfo>>("/languages");

        public async Task<IReadOnlyList<Submission>> ListSubmissionsAsync(string sid, int? exclusive_category, int? inclusive_category, double? min_percent)
            => await Client.GetAsync<List<Submission>>($"/sets/{sid}/submissions?_=_"
                + (exclusive_category.HasValue ? $"&{nameof(exclusive_category)}={exclusive_category}" : string.Empty)
                + (inclusive_category.HasValue ? $"&{nameof(inclusive_category)}={inclusive_category}" : string.Empty)
                + (min_percent.HasValue ? $"&{nameof(min_percent)}={min_percent}" : string.Empty));

        public async Task<IReadOnlyList<PlagiarismSet>> ListSetsAsync(int? related, int? creator, int? skip, int? limit)
            => await Client.GetAsync<List<PlagiarismSet>>($"/sets?_=_"
                + (related.HasValue ? $"&{nameof(related)}={related}" : string.Empty)
                + (creator.HasValue ? $"&{nameof(creator)}={creator}" : string.Empty)
                + (skip.HasValue ? $"&{nameof(skip)}={skip}" : string.Empty)
                + (limit.HasValue ? $"&{nameof(limit)}={limit}" : string.Empty));

        public Task<PlagiarismSet> CreateSetAsync(SetCreation metadata)
            => Client.PostAsync<PlagiarismSet>("/sets", Client.JsonContent(metadata));

        public Task<Submission> SubmitAsync(SubmissionCreation submission)
            => Client.PostAsync<Submission>($"/sets/{submission.SetId}/submissions", Client.JsonContent(submission));

        public Task RescueAsync()
            => Client.PostAsync<ServiceVersion>("/rescue", RestfulClient.EmptyContent);

        public ServiceVersion GetVersion()
        {
            return new ServiceVersion
            {
                BackendVersion = typeof(IBackendRoleStrategy).Assembly.GetName().Version.ToString(),
                Role = "restful"
            };
        }
    }
}
