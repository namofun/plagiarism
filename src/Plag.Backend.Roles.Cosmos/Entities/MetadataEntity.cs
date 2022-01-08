using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Plag.Backend.Entities
{
    public class MetadataEntity
    {
        public const string SettingsTypeKey = "settings";
        public const string LanguagesMetadataKey = "languages";
        public const string SetsTypeKey = "sets";

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }
    }

    public class MetadataEntity<TEntry> : MetadataEntity where TEntry : class
    {
        [JsonProperty("data")]
        public TEntry Data { get; set; }
    }
}
