using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SatelliteSite.Data.Submit
{
    public class Submission
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public ZipFile File{ get; set; }
        public ICollection<Token> tokens { set; get; }
        public string Language { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
