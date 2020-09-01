using Microsoft.AspNetCore.Mvc.DataTables;
using Plag.Backend.Entities;

namespace SatelliteSite.PlagModule.Models
{
    [DtWrapUrl("/dashboard/plagiarism/report/{Id}")]
    public class SubmissionListModel
    {
        public SubmissionListModel(Comparison comparison)
        {
            Id = comparison.Id;
            SubmissionIdAnother = comparison.SubmissionIdAnother;
            SubmissionAnother = comparison.SubmissionAnother;
            Pending = comparison.Pending;

            if (!Pending)
            {
                BiggestMatch = comparison.BiggestMatch;
                TokensMatched = comparison.TokensMatched;
                Percent = comparison.Percent;
                PercentIt = comparison.PercentIt;
                PercentSelf = comparison.PercentSelf;
            }
        }

        [DtIgnore]
        public string Id { get; }

        [DtIcon(8, "fa fa-file-code")]
        [DtWrapUrl("/dashboard/plagiarism/submit/{SubmissionIdAnother}/source-code")]
        public string SubmissionIdAnother { get; }

        [DtDisplay(1, "SID", "{SubmissionAnother} (s{SubmissionIdAnother})", Searchable = true, Sortable = true)]
        public string SubmissionAnother { get; }

        [DtCellCss(Class = "text-variant")]
        [DtBoolSelect("queued", "finished")]
        [DtDisplay(2, "status", Sortable = true)]
        public bool Pending { get; }

        [DtCoalesce("N/A")]
        [DtDisplay(3, "tot.", Sortable = true)]
        public int? TokensMatched { get; }

        [DtCoalesce("N/A")]
        [DtDisplay(4, "big.", Sortable = true)]
        public int? BiggestMatch { get; }

        [DtCoalesce("N/A")]
        [DtDisplay(5, "percent", "{Percent:F2}%", DefaultAscending = "desc", Sortable = true)]
        public double? Percent { get; }

        [DtCoalesce("N/A")]
        [DtDisplay(6, "this", "{PercentSelf:F2}%", Sortable = true)]
        public double? PercentSelf { get; }

        [DtCoalesce("N/A")]
        [DtDisplay(7, "that", "{PercentIt:F2}%", Sortable = true)]
        public double? PercentIt { get; }
    }
}
