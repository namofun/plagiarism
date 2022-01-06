using Plag.Backend.Models;
using System.Text.Json.Serialization;

namespace Plag.Backend.Entities
{
    public class ReportEntity : Report
    {
        [JsonPropertyName("exclusive_category_a")]
        public int ExclusiveCategoryA { get; set; }

        [JsonPropertyName("exclusive_category_b")]
        public int ExclusiveCategoryB { get; set; }

        [JsonPropertyName("submitname_a")]
        public string NameA { get; set; }

        [JsonPropertyName("submitname_b")]
        public string NameB { get; set; }
    }
}
