using Newtonsoft.Json;
using System.Collections.Generic;

namespace Xylab.PlagiarismDetect.Backend.Entities
{
    public class ServiceGraphEntity : MetadataEntity<Dictionary<string, ServiceGraphEntity.Vertex>>
    {
        public ServiceGraphEntity()
        {
            Type = ServiceGraphTypeKey;
            Data = new();
        }

        public class Vertex
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("incl")]
            public int Inclusive { get; set; }

            [JsonProperty("excl")]
            public int Exclusive { get; set; }

            [JsonProperty("lang")]
            public string Language { get; set; }
        }
    }
}
