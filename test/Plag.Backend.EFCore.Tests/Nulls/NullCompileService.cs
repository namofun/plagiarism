using Plag.Backend.Services;
using Plag.Frontend;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Plag.Backend.Tests
{
    public class NullCompileService : ICompileService
    {
        public void Cleanup()
        {
        }

        public ILanguage FindLanguage(string name)
        {
            return null;
        }

        public IEnumerable<ILanguage> GetLanguages()
        {
            return Enumerable.Empty<ILanguage>();
        }

        public bool TryCompile(ILanguage language, ISubmissionFile file, string id, out Submission submission)
        {
            throw new NotImplementedException();
        }
    }
}
