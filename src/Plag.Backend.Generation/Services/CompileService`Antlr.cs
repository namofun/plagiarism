﻿using System.Collections.Generic;
using Xylab.PlagiarismDetect.Frontend;

namespace Xylab.PlagiarismDetect.Backend.Services
{
    public class AntlrCompileService : ICompileService
    {
        public static IReadOnlyDictionary<string, ILanguage> SupportedLanguages { get; }
            = new Dictionary<string, ILanguage>
            {
                ["csharp"] = new Frontend.Csharp.Language(),
                ["cpp"] = new Frontend.Cpp.Language(),
                ["java"] = new Frontend.Java.Language(),
                ["py"] = new Frontend.Python.Language(),
            };

        public void Cleanup()
        {
            foreach (var item in SupportedLanguages.Values)
            {
                item.Cleanup();
            }
        }

        public ILanguage FindLanguage(string name)
        {
            return SupportedLanguages.TryGetValue(name, out var lang) ? lang : null;
        }

        public IEnumerable<ILanguage> GetLanguages()
        {
            return SupportedLanguages.Values;
        }

        public bool TryCompile(ILanguage lang, ISubmissionFile file, string id, out Submission submission)
        {
            submission = new Submission(lang, file, id);
            submission.IL.EnsureNonEmpty();
            return !submission.IL.Errors;
        }
    }
}
