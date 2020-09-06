using Plag.Backend.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Plag.Backend.Services
{
    public class RestfulStoreService : IStoreService
    {
        public static HttpClient Client { get; }

        private async Task<T> SendAsync<T>(HttpRequestMessage request) where T : class
        {
            using var resp = await Client.SendAsync(request);
            if (resp.StatusCode == HttpStatusCode.NotFound)
                return null;
            else if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Created)
                using (var stream = await resp.Content.ReadAsStreamAsync())
                    return await JsonSerializer.DeserializeAsync<T>(stream);
            throw new NotImplementedException();
        }

        private Task<T> GetAsync<T>(string url) where T : class
        {
            return SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, url));
        }

        private Task<T> PostAsync<T>(string url, HttpContent body) where T : class
        {
            return SendAsync<T>(new HttpRequestMessage(HttpMethod.Post, url) { Content = body });
        }

        public Task<LanguageInfo> FindLanguageAsync(string id) => GetAsync<LanguageInfo>($"/api/plag/languages/{id}");

        public Task<Report> FindReportAsync(string id) => GetAsync<Report>($"/api/plag/reports/{id}");

        public Task<PlagiarismSet> FindSetAsync(string id) => GetAsync<PlagiarismSet>($"/api/plag/sets/{id}");

        public Task<Compilation> GetCompilationAsync(string id) => GetAsync<Compilation>($"/api/plag/submissions/{id}/compilation");

        public Task<Submission> FindSubmissionAsync(string id, bool includeFiles = true) => GetAsync<Submission>($"/api/plag/submissions/{id}?includeFiles={includeFiles}");

        public Task<List<Comparison>> GetComparisonsBySubmissionAsync(string id) => GetAsync<List<Comparison>>($"/api/plag/submissions/{id}/comparisons");

        public Task<List<LanguageInfo>> ListLanguageAsync() => GetAsync<List<LanguageInfo>>($"/api/plag/languages");

        public Task<List<Submission>> ListSubmissionsAsync(string set) => GetAsync<List<Submission>>($"/api/plag/submissions?set={set}");

        public Task<List<PlagiarismSet>> ListSetsAsync(int? skip, int? limit)
        {
            var link = "/api/plag/sets?api=1";
            if (skip.HasValue) link += $"&skip={skip.Value}";
            if (limit.HasValue) link += $"&limit={limit.Value}";
            return GetAsync<List<PlagiarismSet>>(link);
        }

        public Task<PlagiarismSet> CreateSetAsync(string name)
        {
            return PostAsync<PlagiarismSet>(
                "/api/plag/sets",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    [nameof(name)] = name
                }));
        }

        public Task<Submission> SubmitAsync(SubmissionCreation submission)
        {
            return PostAsync<Submission>(
                "/api/plag/submissions",
                new ByteArrayContent(JsonSerializer.SerializeToUtf8Bytes(submission)));
        }
    }
}
