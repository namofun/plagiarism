using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SatelliteSite.Data.Match
{
    public class Result
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public ICollection<MatchPair> matchPairs { set; get; }
        public bool ok { set; get; }
        public int tokensMatched { set; get; }
        public int maxMatched { set; get; }

        //SubmissionAB - id
        public string SubmissionA { set; get; }
        public string SubmissionB { set; get; }
        public int SegmentCount => matchPairs.Count;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
