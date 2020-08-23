using System.Collections.Generic;

namespace Plag.Backend.Services
{
    public class AntlrCompileService : ICompileService
    {
        public static IReadOnlyDictionary<string, ILanguage> SupportedLanguages { get; }
            = new Dictionary<string, ILanguage>
            {
                ["csharp8"] = new Frontend.Csharp.Language(),
                ["cpp14"] = new Frontend.Cpp.Language(),
                ["java9"] = new Frontend.Java.Language(),
                ["py3"] = new Frontend.Python.Language(),
            };

        public ILanguage FindLanguage(string name)
        {
            return SupportedLanguages.TryGetValue(name, out var lang) ? lang : null;
        }

        public bool TryCompile(ILanguage lang, ISubmissionFile file, int id, out Submission submission)
        {
            submission = new Submission(lang, file, id);
            return !submission.IL.Errors;
        }
    }
}
