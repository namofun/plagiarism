using System.Collections.Generic;

namespace Plag.Backend
{
    public class LanguageInfo
    {
        public IReadOnlyCollection<string> Suffixes { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public LanguageInfo() { }

        public LanguageInfo(string name, string shortName, IReadOnlyCollection<string> suffixes)
        {
            Name = name;
            ShortName = shortName;
            Suffixes = suffixes;
        }
    }
}
