using System;
using System.Collections.Generic;

namespace Plag
{
    public class Submission
    {
        public Structure IL { get; }

        public ISubmissionFile File { get; }

        public ILanguage Language { get; }

        public string Id { get; }
        
        public Submission(ILanguage lang, ISubmissionFile file)
        {
            Language = lang;
            File = file;
            IL = lang.Parse(file);
            Id = Guid.NewGuid().ToString();
        }

        public Submission(ILanguage lang, ISubmissionFile file, string id, IEnumerable<Token> matches)
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
