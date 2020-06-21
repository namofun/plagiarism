using System;
using System.Collections.Generic;

namespace SatelliteSite
{
    public class PdsRegistry
    {
        public static IReadOnlyDictionary<string, Func<Plag.ILanguage>> SupportedLanguages { get; }
            = new Dictionary<string, Func<Plag.ILanguage>>
            {
                ["csharp8"] = () => new Plag.Frontend.Csharp.Language(),
                ["cpp14"] = () => new Plag.Frontend.Cpp.Language(),
                ["java9"] = () => new Plag.Frontend.Java.Language(),
                ["py3"] = () => new Plag.Frontend.Python.Language(),
            };
    }
}
