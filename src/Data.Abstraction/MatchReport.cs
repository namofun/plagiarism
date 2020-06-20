using Plag;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SatelliteSite.Data
{
    /// <summary>
    /// 查重报告
    /// </summary>
    public class MatchReport
    {
        /// <summary>
        /// 查重报告的编号
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// A提交的GUID
        /// </summary>
        public Guid SubmissionA { get; set; }

        /// <summary>
        /// B提交的GUID
        /// </summary>
        public Guid SubmissionB { get; set; }

        /// <summary>
        /// 相似代码对集合
        /// </summary>
        public ICollection<MatchPair> MatchPairs { get; set; }

        /// <summary>
        /// 总计匹配成功的Token长度
        /// </summary>
        public int TokensMatched { get; set; }

        /// <summary>
        /// 最长连续匹配成功的Token长度
        /// </summary>
        public int BiggestMatch { get; set; }

        /// <summary>
        /// 抄袭百分比
        /// </summary>
        public double Percent { get; set; }

        /// <summary>
        /// A的抄袭百分比
        /// </summary>
        public double PercentA { get; set; }

        /// <summary>
        /// B的抄袭百分比
        /// </summary>
        public double PercentB { get; set; }

        /// <summary>
        /// AB两者抄袭百分比最大值
        /// </summary>
        public double PercentMaxAB { get; set; }

        /// <summary>
        /// AB两者抄袭百分比最小值
        /// </summary>
        public double PercentMinAB { get; set; }

        /// <summary>
        /// 从Matching创建报告
        /// </summary>
        /// <param name="matching"></param>
        /// <returns></returns>
        public static MatchReport Create(Matching matching)
        {
            return new MatchReport
            {
                Id = Guid.NewGuid(),
                TokensMatched = matching.TokensMatched,
                BiggestMatch = matching.BiggestMatch,
                Percent = matching.Percent,
                PercentA = matching.PercentA,
                PercentB = matching.PercentB,
                PercentMaxAB = matching.PercentMaxAB,
                PercentMinAB = matching.PercentMinAB,
                SubmissionA = matching.SubmissionA.Id,
                SubmissionB = matching.SubmissionB.Id,

                MatchPairs = matching.Select((i, j) => new MatchPair
                {
                    MatchingId = j,
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
