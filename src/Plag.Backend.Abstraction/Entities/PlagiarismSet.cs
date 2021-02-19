using System;

namespace Plag.Backend.Entities
{
    public class PlagiarismSet<TKey> where TKey : IEquatable<TKey>
    {
        public TKey Id { get; set; }

        public int? UserId { get; set; }

        public int? ContestId { get; set; }

        public DateTimeOffset CreateTime { get; set; }

        public string Name { get; set; }

        public int ReportCount { get; set; }

        public int ReportPending { get; set; }

        public Models.PlagiarismSet ToModel()
        {
            return new Models.PlagiarismSet
            {
                Id = Id.ToString(),
                CreateTime = CreateTime,
                Name = Name,
                ReportCount = ReportCount,
                ReportPending = ReportPending,
                ContestId = ContestId,
                UserId = UserId,
            };
        }
    }
}
