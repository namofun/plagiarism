using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Xylab.PlagiarismDetect.Backend.Services
{
    public class RestfulClient
    {
        public static HttpContent EmptyContent { get; } = new ByteArrayContent(Array.Empty<byte>());

        public HttpClient Client { get; }

        public PlagRestfulOptions Options { get; }

        public RestfulClient(HttpClient client, IOptions<PlagRestfulOptions> options)
        {
            Client = client;
            Options = options.Value;
        }

        private async Task<T> SendAsync<T>(HttpRequestMessage request) where T : class
        {
            if (Options.Preprocessor != null)
            {
                await Options.Preprocessor.Invoke(request);
            }

            using var resp = await Client.SendAsync(request);

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException();
            }
            else if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Created)
            {
                using var stream = await resp.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<T>(stream, Options.JsonSerializerOptions);
            }
            else if (resp.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentOutOfRangeException();
            }
            else if (resp.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new ApplicationException();
            }
            else if (resp.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                throw new HttpRequestException();
            }

            throw new NotImplementedException();
        }

        public Task<T> GetAsync<T>(string url) where T : class
        {
            return SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, "/api/plagiarism" + url));
        }

        public Task<T> PostAsync<T>(string url, HttpContent body) where T : class
        {
            return SendAsync<T>(new HttpRequestMessage(HttpMethod.Post, "/api/plagiarism" + url) { Content = body });
        }

        public Task<T> DeleteAsync<T>(string url) where T : class
        {
            return SendAsync<T>(new HttpRequestMessage(HttpMethod.Delete, "/api/plagiarism" + url));
        }

        public Task<T> PatchAsync<T>(string url, HttpContent body) where T : class
        {
            return SendAsync<T>(new HttpRequestMessage(new HttpMethod("PATCH"), "/api/plagiarism" + url) { Content = body });
        }

        public static HttpContent JsonContent<T>(T value)
        {
            var content = JsonSerializer.SerializeToUtf8Bytes(value);
            var body = new ByteArrayContent(content);
            body.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            return body;
        }
    }
}
