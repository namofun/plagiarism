using System.Text.Json.Serialization;

namespace Plag.Backend.Models
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
