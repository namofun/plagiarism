using System;
using System.Text.Json.Serialization;
using Xylab.PlagiarismDetect.Backend.Models;

namespace Xylab.PlagiarismDetect.Backend.Entities
{
    public class SubmissionFile<TKey> : SubmissionFile where TKey : IEquatable<TKey>
    {
        [JsonIgnore]
        public TKey SubmissionId { get; set; }
    }
}
