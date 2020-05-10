using System;
using System.IO;

namespace Plag
{
    public class Submission
    {
        public Structure IL { get; }

        public string FileName { get; }

        public ILanguage Language { get; }

        public int FileSize { get; }

        public Submission(ILanguage lang, string fileName, int size, Func<Stream> func)
        {
            IL = lang.Parse(fileName, func);
            FileName = fileName;
            Language = lang;
            FileSize = size;
        }
    }
}
