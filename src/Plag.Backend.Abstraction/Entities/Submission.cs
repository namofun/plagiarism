using System;
using System.Collections.Generic;

namespace Plag.Backend.Entities
{
    public class Submission
    {
        public int Id { get; set; }

        public int SetId { get; set; }

        public string Name { get; set; }

        public double MaxPercent { get; set; }

        public bool? TokenProduced { get; set; }

        public DateTimeOffset UploadTime { get; set; }

        public ICollection<SubmissionFile> Files { get; set; }
        
        public Plag.Submission Tokens { get; set; }
        
        public string Language { get; set; }
    }
}
