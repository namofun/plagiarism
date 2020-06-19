using System.Collections.Generic;
using System;

namespace SatelliteSite.Data.Check
{
    /// <summary>
    /// TeamReport
    /// </summary>
    public class TeamReport
    {
        /// <summary>
        /// SubmissionA GUID
        /// </summary>
        public string SubmissionA { set; get; }
        /// <summary>
        /// 作者名字
        /// </summary>
        public string AuthorName { set; get; }
        /// <summary>
        /// 最大查重率
        /// </summary>
        public double MaxPercent { set; get; }
        /// <summary>
        /// 所有相关的Report
        /// </summary>
        public ICollection<MatchReport> matchReports { set; get; }

    }
}
