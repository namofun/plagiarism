using System;
using System.Collections.Generic;

namespace Plag.Backend.Entities
{
    public class PlagiarismSet
    {
        public string Id { get; set; }

        public DateTimeOffset CreateTime { get; set; }

        public string Name { get; set; }

        public int ReportCount { get; set; }

        public int ReportPending { get; set; }

        public ICollection<Submission> Submissions { get; set; }
    }
}
