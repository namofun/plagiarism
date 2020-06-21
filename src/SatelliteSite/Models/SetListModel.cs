using Microsoft.AspNetCore.Mvc.DataTables;
using System;

namespace SatelliteSite.Models
{
    [DtWrapUrl("/plagiarism/set/{Id}")]
    public class SetListModel
    {
        [DtDisplay(0, "ID", Sortable = true)]
        public string Id { get; set; }

        [DtDisplay(1, "name", Sortable = true)]
        public string Name { get; set; }

        [DtDisplay(2, "time", Sortable = true)]
        public DateTimeOffset CreateTime { get; set; }

        [DtDisplay(3, "progress", "{Progress:F2}% ({ResolvedReports}/{TotalReports})", Sortable = true)]
        public double Progress => TotalReports == 0 ? 0 : 100.0 * (TotalReports - PendingReports) / TotalReports;

        [DtIgnore]
        public int TotalReports { get; set; }

        [DtIgnore]
        public int PendingReports { get; set; }

        [DtIgnore]
        public int ResolvedReports => TotalReports - PendingReports;
    }
}
