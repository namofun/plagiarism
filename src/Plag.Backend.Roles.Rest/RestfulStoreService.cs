using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Models;

namespace Xylab.PlagiarismDetect.Backend.Services
{
    public class RestfulStoreService : IPlagiarismDetectService
    {
        public RestfulClient Client { get; }

        public RestfulStoreService(RestfulClient client)
            => Client = client;

        public Task<LanguageInfo> FindLanguageAsync(string id)
            => Client.GetAsync<LanguageInfo>(
                $"/languages/{UrlEncoder.Default.Encode(id)}");

        public Task<Report> FindReportAsync(string id)
            => Client.GetAsync<Report>(
                $"/reports/{UrlEncoder.Default.Encode(id)}");

        public Task<PlagiarismSet> FindSetAsync(string sid)
            => Client.GetAsync<PlagiarismSet>(
                $"/sets/{UrlEncoder.Default.Encode(sid)}");

        public Task<Compilation> GetCompilationAsync(string sid, int id)
            => Client.GetAsync<Compilation>(
                $"/sets/{UrlEncoder.Default.Encode(sid)}/submissions/{id}/compilation");

        public Task ResetCompilationAsync(string sid, int id)
            => Client.DeleteAsync<ServiceVersion>(
                $"/sets/{UrlEncoder.Default.Encode(sid)}/submissions/{id}/compilation");

        public Task<Submission> FindSubmissionAsync(string sid, int id, bool includeFiles = true)
            => Client.GetAsync<Submission>(
                $"/sets/{UrlEncoder.Default.Encode(sid)}/submissions/{id}?includeFiles={includeFiles}");

        public async Task<Vertex> GetComparisonsBySubmissionAsync(string sid, int id, bool includeFiles = false)
            => await Client.GetAsync<Vertex>(
                $"/sets/{UrlEncoder.Default.Encode(sid)}/submissions/{id}/comparisons?includeFiles={includeFiles}");

        public async Task<IReadOnlyList<SubmissionFile>> GetFilesAsync(string sid, int id)
            => await Client.GetAsync<List<SubmissionFile>>(
                $"/sets/{UrlEncoder.Default.Encode(sid)}/submissions/{id}/files");

        public Task<List<LanguageInfo>> ListLanguageAsync()
            => Client.GetAsync<List<LanguageInfo>>(
                "/languages");

        public async Task<IReadOnlyList<Submission>> ListSubmissionsAsync(string sid, string language, int? exclusive_category, int? inclusive_category, double? min_percent, int? skip, int? limit, string order, bool asc)
            => new[] { "id", "percent" }.Contains(order = (order ?? "id").ToLowerInvariant())
                ? await Client.GetAsync<List<Submission>>(
                    $"/sets/{UrlEncoder.Default.Encode(sid)}/submissions?_=_"
                        + (!string.IsNullOrWhiteSpace(language) ? $"&{nameof(language)}={UrlEncoder.Default.Encode(language)}" : string.Empty)
                        + (exclusive_category.HasValue ? $"&{nameof(exclusive_category)}={exclusive_category}" : string.Empty)
                        + (inclusive_category.HasValue ? $"&{nameof(inclusive_category)}={inclusive_category}" : string.Empty)
                        + (min_percent.HasValue ? $"&{nameof(min_percent)}={min_percent}" : string.Empty)
                        + (!asc ? "&order=desc" : string.Empty)
                        + (order != "id" ? "&by=" + order : string.Empty)
                        + (skip.HasValue ? $"&{nameof(skip)}={skip}" : string.Empty)
                        + (limit.HasValue ? $"&{nameof(limit)}={limit}" : string.Empty))
                : throw new ArgumentOutOfRangeException();

        public async Task<IReadOnlyList<PlagiarismSet>> ListSetsAsync(int? related, int? creator, int? skip, int? limit, bool asc)
            => await Client.GetAsync<List<PlagiarismSet>>(
                $"/sets?_=_"
                    + (related.HasValue ? $"&{nameof(related)}={related}" : string.Empty)
                    + (creator.HasValue ? $"&{nameof(creator)}={creator}" : string.Empty)
                    + (asc ? "&order=asc" : string.Empty)
                    + (skip.HasValue ? $"&{nameof(skip)}={skip}" : string.Empty)
                    + (limit.HasValue ? $"&{nameof(limit)}={limit}" : string.Empty));

        public Task<PlagiarismSet> CreateSetAsync(SetCreation metadata)
            => Client.PostAsync<PlagiarismSet>(
                "/sets",
                RestfulClient.JsonContent(metadata));

        public Task<Submission> SubmitAsync(SubmissionCreation submission)
            => Client.PostAsync<Submission>(
                $"/sets/{UrlEncoder.Default.Encode(submission.SetId)}/submissions",
                RestfulClient.JsonContent(submission));

        public Task RescueAsync()
            => Client.PostAsync<ServiceVersion>(
                "/rescue",
                RestfulClient.EmptyContent);

        public Task JustificateAsync(string reportid, ReportJustification status)
            => Client.PatchAsync<Dictionary<string, string>>(
                $"/reports/{UrlEncoder.Default.Encode(reportid)}",
                new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("justification", status.ToString()) }));

        public Task ShareReportAsync(string reportid, bool shared)
            => Client.PatchAsync<Dictionary<string, string>>(
                $"/reports/{UrlEncoder.Default.Encode(reportid)}",
                new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("shared", shared.ToString().ToLower()) }));

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
