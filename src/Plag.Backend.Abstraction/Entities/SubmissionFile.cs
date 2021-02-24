using Plag.Backend.Models;
using System;
using System.Text.Json.Serialization;

namespace Plag.Backend.Entities
{
    public class SubmissionFile<TKey> : SubmissionFile where TKey : IEquatable<TKey>
    {
        [JsonIgnore]
        public TKey SubmissionId { get; set; }
    }
}
