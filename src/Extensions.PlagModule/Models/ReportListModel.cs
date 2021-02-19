using Microsoft.AspNetCore.Mvc.DataTables;
using Plag.Backend.Models;
using System;

namespace SatelliteSite.PlagModule.Models
{
    [DtWrapUrl("/dashboard/plagiarism/{SetId}/submissions/{Id}")]
    public class ReportListModel
    {
        [DtIgnore]
        public string SetId { get; set; }

        [DtDisplay(0, "ID", Searchable = true, Sortable = true)]
        public int Id { get; set; }

        [DtDisplay(1, "name", Searchable = true, Sortable = true)]
        public string Name { get; set; }

        [DtDisplay(2, "status", Sortable = true)]
        [DtCellCss(Class = "text-variant")]
        public string Status { get; set; }

        [DtDisplay(3, "upload time", "{Time:yyyy/MM/dd hh:mm:ss}", Sortable = true)]
        public DateTimeOffset Time { get; set; }

        [DtDisplay(4, "exc.", Sortable = true)]
        public int Exclusive { get; set; }

        [DtDisplay(5, "inc.", Sortable = true)]
        public int Inclusive { get; set; }

        [DtDisplay(6, "max percent", "{Percent:F2}%", Sortable = true)]
        public double Percent { get; set; }

        [DtIcon(7, "fas fa-file-code")]
        [DtWrapUrl("/dashboard/plagiarism/{SetId}/submissions/{Id}/source-code")]
        public object ViewSubmission { set { } }

        public static ReportListModel Conv(Submission s)
        {
            return new ReportListModel
            {
                SetId = s.SetId,
                Id = s.Id,
                Name = s.Name,
                Percent = s.MaxPercent,
                Time = s.UploadTime,
                Inclusive = s.InclusiveCategory,
                Exclusive = s.ExclusiveCategory,
                Status = !s.TokenProduced.HasValue
                    ? "waiting"
                    : s.TokenProduced.Value
                    ? "ready"
                    : "failed"
            };
        }
    }
}
