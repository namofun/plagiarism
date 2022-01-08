namespace Plag.Backend.Models
{
    public class ReportTask<TKey> : ReportTask
    {
        public new TKey Id { get; }

        public new TKey SetId { get; }

        public ReportTask(TKey reportid, TKey setid, int a, int b)
            : base(reportid.ToString(), setid.ToString(), a, b)
        {
            Id = reportid;
            SetId = setid;
        }

        public static ReportTask<TKey> Of(TKey reportid, TKey setid, int a, int b)
        {
            return new ReportTask<TKey>(reportid, setid, a, b);
        }
    }
}
