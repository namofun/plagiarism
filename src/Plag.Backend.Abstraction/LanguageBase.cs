using System.Collections.Generic;

namespace Plag.Backend
{
    public class LanguageInfo
    {
        public IReadOnlyCollection<string> Suffixes { get; }

        public string Name { get; }

        public string ShortName { get; }

        public LanguageInfo(string name, string shortName, IReadOnlyCollection<string> suffixes)
        {
            Name = name;
            ShortName = shortName;
            Suffixes = suffixes;
        }
    }
}
