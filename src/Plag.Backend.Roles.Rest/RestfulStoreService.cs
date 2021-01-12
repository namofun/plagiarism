using Plag.Backend.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Plag.Backend.Services
{
    public class RestfulStoreService : IStoreService
    {
        public HttpClient Client { get; }

        public JsonSerializerOptions Options { get; }

        public RestfulStoreService(HttpClient client)
        {
            Client = client;
            Options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        private async Task<T> SendAsync<T>(HttpRequestMessage request) where T : class
        {
            using var resp = await Client.SendAsync(request);

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            else if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Created)
            {
                using var stream = await resp.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<T>(stream, Options);
            }

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

        public Task<LanguageInfo> FindLanguageAsync(string id) => GetAsync<LanguageInfo>($"/api/plagiarism/languages/{id}");

        public Task<Report> FindReportAsync(string id) => GetAsync<Report>($"/api/plagiarism/reports/{id}");

        public Task<PlagiarismSet> FindSetAsync(string id) => GetAsync<PlagiarismSet>($"/api/plagiarism/sets/{id}");

        public Task<Compilation> GetCompilationAsync(string id) => GetAsync<Compilation>($"/api/plagiarism/submissions/{id}/compilation");

        public Task<Submission> FindSubmissionAsync(string id, bool includeFiles = true) => GetAsync<Submission>($"/api/plagiarism/submissions/{id}?includeFiles={includeFiles}");

        public Task<List<Comparison>> GetComparisonsBySubmissionAsync(string id) => GetAsync<List<Comparison>>($"/api/plagiarism/submissions/{id}/comparisons");

        public Task<List<LanguageInfo>> ListLanguageAsync() => GetAsync<List<LanguageInfo>>($"/api/plagiarism/languages");

        public Task<List<Submission>> ListSubmissionsAsync(string set) => GetAsync<List<Submission>>($"/api/plagiarism/sets/{set}/submissions");

        public Task<List<PlagiarismSet>> ListSetsAsync(int? skip, int? limit)
        {
            var link = "/api/plagiarism/sets?api=1";
            if (skip.HasValue) link += $"&skip={skip.Value}";
            if (limit.HasValue) link += $"&limit={limit.Value}";
            return GetAsync<List<PlagiarismSet>>(link);
        }

        public Task<PlagiarismSet> CreateSetAsync(string name)
        {
            return PostAsync<PlagiarismSet>(
                "/api/plagiarism/sets",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    [nameof(name)] = name
                }));
        }

        public Task<Submission> SubmitAsync(SubmissionCreation submission)
        {
            var content = JsonSerializer.SerializeToUtf8Bytes(submission);
            var body = new ByteArrayContent(content);
            body.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            return PostAsync<Submission>("/api/plagiarism/submissions", body);
        }
    }
}
