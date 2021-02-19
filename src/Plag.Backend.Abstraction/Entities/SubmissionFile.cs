using Plag.Backend.Models;
using System;
using System.Text.Json.Serialization;

namespace Plag.Backend.Entities
{
    public class SubmissionFile<TKey> : SubmissionFile where TKey : IEquatable<TKey>
    {
        [JsonIgnore]
        public TKey SetId { get; set; }

        [JsonIgnore]
        public int SubmissionId { get; set; }
    }
}
