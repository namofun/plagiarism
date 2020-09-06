using System.Collections.Generic;

namespace Plag.Backend
{
    public class SubmissionCreation
    {
        public string SetId { get; set; }

        public string Name { get; set; }

        public string Language { get; set; }

        public ICollection<SubmissionFileCreation> Files { get; set; }

        public class SubmissionFileCreation
        {
            public string FilePath { get; set; }

            public string FileName { get; set; }

            public string Content { get; set; }
        }
    }
}
