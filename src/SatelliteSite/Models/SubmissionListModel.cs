using Microsoft.AspNetCore.Mvc.DataTables;
using System;

namespace SatelliteSite.Models
{
    [DtWrapUrl("/submission/{Id}")]
    public class SubmissionListModel
    {
        [DtDisplay(0, "StuName", Sortable = true)]
        public string StuName { get; set; }

        [DtDisplay(1, "ID", Sortable = true, DefaultAscending = "asc")]
        public string Id { get; set; }

        [DtDisplay(2, "UploadTime", Sortable = true)]
        public DateTimeOffset UploadTime { get; set; }

        [DtDisplay(3, "lang", Sortable = true)]
        public string Language { get; set; }
    }
}
