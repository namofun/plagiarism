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
        public List<File> Files { get; set; }
        public ICollection<Token> Tokens { set; get; }
        public string Language { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
