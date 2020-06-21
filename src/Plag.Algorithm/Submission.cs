using System.Collections.Generic;

namespace Plag
{
    public class Submission
    {
        public Structure IL { get; }

        public ISubmissionFile File { get; }

        public ILanguage Language { get; }

        public int Id { get; }
        
        public Submission(ILanguage lang, ISubmissionFile file, int id = 0, IEnumerable<Token> matches = null)
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
