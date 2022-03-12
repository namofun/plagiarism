using System.Collections.Generic;

namespace Xylab.PlagiarismDetect.Frontend
{
    public class Submission
    {
        public Structure IL { get; }

        public ISubmissionFile File { get; }

        public ILanguage Language { get; }

        public string Id { get; }
        
        public Submission(ILanguage lang, ISubmissionFile file, string id = null, IEnumerable<Token> matches = null)
        {
            Language = lang;
            File = file;
            Id = id;

            if (matches == null)
            {
                IL = lang.Parse(file);
            }
            else
            {
                IL = new Structure();
                IL.AddTokens(matches);
            }
        }
    }
}
