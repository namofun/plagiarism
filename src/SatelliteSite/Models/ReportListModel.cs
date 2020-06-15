using Microsoft.AspNetCore.Mvc.DataTables;

namespace SatelliteSite.Models
{
    [DtWrapUrl("/report/{Id}")]
    public class ReportListModel
    {
        [DtDisplay(0, "Id", Sortable = true, DefaultAscending = "asc")]
        public string Id { get; set; }

        [DtDisplay(1, "SubmissionA", Sortable = true)]
        public string SubmissionA { get; set; }

        [DtDisplay(2, "SubmissionB", Sortable = true)]
        public string SubmissionB { get; set; }

        [DtDisplay(3, "Percent", Sortable = true)]
        public double Percent { get; set; }
    }
}
