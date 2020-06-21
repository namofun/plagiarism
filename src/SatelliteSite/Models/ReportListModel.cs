using Microsoft.AspNetCore.Mvc.DataTables;
using SatelliteSite.Data;
using System;

namespace SatelliteSite.Models
{
    [DtWrapUrl("/plagiarism/submit/{Id}")]
    public class ReportListModel
    {
        [DtDisplay(0, "ID", Searchable = true, Sortable = true)]
        public int Id { get; set; }

        [DtDisplay(1, "name", Searchable = true, Sortable = true)]
        public string Name { get; set; }

        [DtDisplay(2, "status", Sortable = true)]
        [DtCellCss(Class = "text-variant")]
        public string Status { get; set; }

        [DtDisplay(3, "upload time", "{Time:yyyy/MM/dd hh:mm:ss}", Sortable = true)]
        public DateTimeOffset Time { get; set; }

        [DtDisplay(4, "max percent", "{Percent:F2}%", Sortable = true)]
        public double Percent { get; set; }

        [DtIcon(5, "fas fa-file-code")]
        [DtWrapUrl("/plagiarism/submit/{Id}/source-code")]
        public object ViewSubmission { set { } }

        public static ReportListModel Conv(Submission s)
        {
            return new ReportListModel
            {
                Id = s.Id,
                Name = s.Name,
                Percent = s.MaxPercent,
                Time = s.UploadTime,
                Status = !s.TokenProduced.HasValue
                    ? "waiting"
                    : s.TokenProduced.Value
                    ? "ready"
                    : "failed"
            };
        }
    }
}
