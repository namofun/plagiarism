using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Plag.Backend.Entities
{
    public class MetadataEntity
    {
        public const string LanguagesMetadataKey = "languages";

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }
    }

    public class MetadataEntity<TEntry> : MetadataEntity where TEntry : class
    {
        [JsonProperty("data")]
        public TEntry Data { get; set; }
    }
}
