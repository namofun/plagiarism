using System;
using System.IO;

namespace Plag
{
    public class Submission
    {
        public Structure IL { get; }

        public ISubmissionFile File { get; }

        public ILanguage Language { get; }
        
        public Submission(ILanguage lang, ISubmissionFile file)
        {
            Language = lang;
            File = file;
            IL = lang.Parse(file);
        }
    }
}
