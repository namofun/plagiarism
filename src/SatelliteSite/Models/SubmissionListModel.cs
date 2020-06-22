using Microsoft.AspNetCore.Mvc.DataTables;
using System;

namespace SatelliteSite.Models
{
    [DtWrapUrl("/plagiarism/report/{Id}")]
    public class SubmissionListModel
    {
        [DtIgnore]
        public Guid Id { get; set; }

        [DtIcon(8, "fa fa-file-code")]
        [DtWrapUrl("/plagiarism/submit/{SubmissionIdAnother}/source-code")]
        public int SubmissionIdAnother { get; set; }

        [DtDisplay(1, "SID", "{SubmissionAnother} (s{SubmissionIdAnother})", Searchable = true, Sortable = true)]
        public string SubmissionAnother { get; set; }

        [DtCellCss(Class = "text-variant")]
        [DtBoolSelect("queued", "finished")]
        [DtDisplay(2, "status", Sortable = true)]
        public bool Pending { get; set; }

        [DtCoalesce("N/A")]
        [DtDisplay(3, "tot.", Sortable = true)]
        public int? TokensMatched { get; set; }

        [DtCoalesce("N/A")]
        [DtDisplay(4, "big.", Sortable = true)]
        public int? BiggestMatch { get; set; }

        [DtCoalesce("N/A")]
        [DtDisplay(5, "percent", "{Percent:F2}%", DefaultAscending = "desc", Sortable = true)]
        public double? Percent { get; set; }

        [DtCoalesce("N/A")]
        [DtDisplay(6, "this", "{PercentSelf:F2}%", Sortable = true)]
        public double? PercentSelf { get; set; }

        [DtCoalesce("N/A")]
        [DtDisplay(7, "that", "{PercentIt:F2}%", Sortable = true)]
        public double? PercentIt { get; set; }

        public void EnsurePending()
        {
            if (Pending)
            {
                BiggestMatch = TokensMatched = null;
                Percent = PercentIt = PercentSelf = null;
            }
        }
    }
}
