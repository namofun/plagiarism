using System.Text.Json.Serialization;
using Xylab.PlagiarismDetect.Backend.Models;

namespace Xylab.PlagiarismDetect.Backend.Entities
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

        [JsonPropertyName("type")]
        public string Type { get; set; } = "report";

        [JsonPropertyName("session_lock")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string SessionLock { get; set; }

        public static ReportEntity Of(
            SetGuid set,
            (int id, string name, int excl) a,
            (int id, string name, int excl) b)
        {
            if (a.id < b.id)
            {
                var c = a;
                a = b;
                b = c;
            }

            return new()
            {
                SetId = set.ToString(),
                SubmissionA = a.id,
                SubmissionB = b.id,
                NameA = a.name,
                NameB = b.name,
                ExclusiveCategoryA = a.excl,
                ExclusiveCategoryB = b.excl,
                Id = ReportGuid.FromStructured(set, a.id, b.id).ToString(),
            };
        }
    }
}
