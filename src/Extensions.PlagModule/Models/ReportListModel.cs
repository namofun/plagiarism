using Plag.Backend.Models;
using System;

namespace SatelliteSite.PlagModule.Models
{
    public class ReportListModel
    {
        public string SetId { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Status { get; set; }

        public DateTimeOffset Time { get; set; }

        public int Exclusive { get; set; }

        public int Inclusive { get; set; }

        public string Language { get; set; }

        public double Percent { get; set; }

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
                Language = s.Language,
                Status = !s.TokenProduced.HasValue
                    ? "waiting"
                    : s.TokenProduced.Value
                    ? "ready"
                    : "failed"
            };
        }
    }
}
