using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Plag.Backend.Models
{
    public class LanguageInfo
    {
        [JsonPropertyName("suffixes")]
        public IReadOnlyCollection<string> Suffixes { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("short_name")]
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
