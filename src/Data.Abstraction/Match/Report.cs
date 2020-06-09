using Newtonsoft.Json;
using Plag;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SatelliteSite.Data.Match
{
    /// <summary>
    /// 查重报告
    /// </summary>
    public class Report
    {
        /// <summary>
        /// 查重报告的编号
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// 相似代码对集合
        /// </summary>
        public ICollection<MatchPair> MatchPairs { set; get; }

        /// <summary>
        /// A提交的GUID
        /// </summary>
        public string SubmissionA { set; get; }

        /// <summary>
        /// B提交的GUID
        /// </summary>
        public string SubmissionB { set; get; }

        /// <summary>
        /// 总计匹配成功的Token长度
        /// </summary>
        public int TokensMatched { set; get; }

        /// <summary>
        /// 最长连续匹配成功的Token长度
        /// </summary>
        public int BiggestMatch { set; get; }

        /// <summary>
        /// 抄袭百分比
        /// </summary>
        public double Percent { set; get; }

        /// <summary>
        /// A的抄袭百分比
        /// </summary>
        public double PercentA { set; get; }

        /// <summary>
        /// B的抄袭百分比
        /// </summary>
        public double PercentB { set; get; }

        /// <summary>
        /// AB两者抄袭百分比最大值
        /// </summary>
        public double PercentMaxAB { set; get; }

        /// <summary>
        /// AB两者抄袭百分比最小值
        /// </summary>
        public double PercentMinAB { set; get; }

        public static Report Create(Matching matching)
        {
            int t = 0;
            return new Report
            {
                Id = Guid.NewGuid().ToString(),
                TokensMatched = matching.TokensMatched,
                BiggestMatch = matching.BiggestMatch,
                Percent = matching.Percent,
                PercentA = matching.PercentA,
                PercentB = matching.PercentB,
                PercentMaxAB = matching.PercentMaxAB,
                PercentMinAB = matching.PercentMinAB,
                SubmissionA = matching.SubmissionA.Id,
                SubmissionB = matching.SubmissionB.Id,
                MatchPairs = matching.Select(i => new MatchPair
                {
                    Mid = t++,
                    ContentStartA = matching.SubmissionA.IL[i.StartA].Column,
                    ContentEndA = matching.SubmissionA.IL[i.StartA + i.Length].Column,
                    ContentStartB = matching.SubmissionB.IL[i.StartB].Column,
                    ContentEndB = matching.SubmissionB.IL[i.StartB + i.Length].Column,
                    FileA = matching.SubmissionA.IL[i.StartA].FileId,
                    FileB = matching.SubmissionB.IL[i.StartB].FileId,
                })
                .ToList()
            };
        }
    }
}
