using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Plag;
namespace SatelliteSite.Data.Match
{
    public class Result
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public ICollection<MatchPair> matchPairs { set; get; }

        //SubmissionAB - id(GUID)
        public string SubmissionA { set; get; }
        public string SubmissionB { set; get; }
        public int SegmentCount => matchPairs.Count;

        public int TokensMatched { set; get; }

        public int BiggestMatch { set; get; }

        public double Percent { set; get; }

        public double PercentA { set; get; }

        public double PercentB { set; get; }

        public double PercentMaxAB { set; get; }

        public double PercentMinAB { set; get; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
        public Result(Matching matching)
        {
            TokensMatched = matching.TokensMatched;
            BiggestMatch = matching.BiggestMatch;
            Percent = matching.Percent;
            PercentA = matching.PercentA;
            PercentB = matching.PercentB;
            PercentMaxAB = matching.PercentMaxAB;
            PercentMinAB = matching.PercentMinAB;
            foreach (var i in matching)
            {
                var beginA = matching.SubmissionA.IL[i.StartA].Column;
                var endA = matching.SubmissionA.IL[i.StartA + i.Length].Column;
                var beginB = matching.SubmissionB.IL[i.StartB].Column;
                var endB = matching.SubmissionB.IL[i.StartB + i.Length].Column;
                matchPairs.Add(new MatchPair(i.StartA, i.StartB, i.Length, beginA, endA, beginB, endB));
            }
        }
    }
}
