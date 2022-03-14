using System;

namespace SatelliteSite.PlagModule.Models
{
    public class SetListModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public DateTimeOffset CreateTime { get; set; }

        public long TotalReports { get; set; }

        public long PendingReports { get; set; }

        public int TotalSubmissions { get; set; }

        public int FinishedSubmissions { get; set; }
    }
}
