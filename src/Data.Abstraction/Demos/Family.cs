using Newtonsoft.Json;
using System.Collections.Generic;

namespace SatelliteSite.Data.Demos
{
    public class Family
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string LastName { get; set; }
        public ICollection<Parent> Parents { get; set; }
        public ICollection<Child> Children { get; set; }
        public Address Address { get; set; }
        public bool IsRegistered { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
