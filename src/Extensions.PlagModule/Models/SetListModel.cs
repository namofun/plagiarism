using System;

namespace SatelliteSite.PlagModule.Models
{
    public class SetListModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public DateTimeOffset CreateTime { get; set; }

        public int TotalReports { get; set; }

        public int PendingReports { get; set; }

        public int TotalSubmissions { get; set; }

        public int FinishedSubmissions { get; set; }
    }
}
