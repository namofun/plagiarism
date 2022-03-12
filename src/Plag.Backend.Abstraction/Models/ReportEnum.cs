using System.Text.Json.Serialization;

namespace Xylab.PlagiarismDetect.Backend.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReportJustification
    {
        Unspecified,
        Claimed,
        Ignored,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReportState
    {
        Pending,
        Analyzing,
        Finished,
    }
}
