using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Plag.Backend.Entities
{
    public class Submission<TKey> where TKey : IEquatable<TKey>
    {
        public TKey SetId { get; set; }

        public int Id { get; set; }

        public TKey ExternalId { get; set; }

        public int ExclusiveCategory { get; set; }

        public int InclusiveCategory { get; set; }

        public string Name { get; set; }

        public double MaxPercent { get; set; }

        public bool? TokenProduced { get; set; }

        public DateTimeOffset UploadTime { get; set; }

        public string Language { get; set; }

        public string Error { get; set; }

        public byte[] Tokens { get; set; }

        public Models.Submission ToModel(IReadOnlyCollection<Models.SubmissionFile> files = null)
        {
            return new Models.Submission
            {
                MaxPercent = MaxPercent,
                ExclusiveCategory = ExclusiveCategory,
                ExternalId = ExternalId.ToString(),
                Files = files,
                Id = Id,
                InclusiveCategory = InclusiveCategory,
                Language = Language,
                Name = Name,
                SetId = SetId.ToString(),
                TokenProduced = TokenProduced,
                UploadTime = UploadTime,
            };
        }

        public static readonly Expression<Func<Submission<TKey>, Submission<TKey>>> Minify
            = s => new Submission<TKey>
            {
                ExclusiveCategory = s.ExclusiveCategory,
                ExternalId = s.ExternalId,
                SetId = s.SetId,
                Id = s.Id,
                InclusiveCategory = s.InclusiveCategory,
                Language = s.Language,
                MaxPercent = s.MaxPercent,
                Name = s.Name,
                TokenProduced = s.TokenProduced,
                UploadTime = s.UploadTime,
            };
    }
}
