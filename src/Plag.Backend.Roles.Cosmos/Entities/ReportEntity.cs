using Plag.Backend.Models;
using System.Text.Json.Serialization;

namespace Plag.Backend.Entities
{
    internal class ReportEntity : Report
    {
        [JsonPropertyName("id")]
        public string InternalId { get; set; }
    }
}
