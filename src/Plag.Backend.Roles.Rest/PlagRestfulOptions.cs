#nullable enable
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Xylab.PlagiarismDetect.Backend
{
    public class PlagRestfulOptions
    {
        public string? ServerAddress { get; set; }

        public Func<HttpRequestMessage, Task>? Preprocessor { get; set; }

        public JsonSerializerOptions JsonSerializerOptions { get; set; } = new() { PropertyNameCaseInsensitive = true };
    }
}
