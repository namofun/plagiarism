using System;
using System.Collections.Generic;
using System.Text;

namespace SatelliteSite.Data.Check
{
    public class MatchReport
    {
        /// <summary>
        /// SubmissionB GUID
        /// </summary>
        public string SubmissionB { set; get; }
        /// <summary>
        /// Report GUID
        /// </summary>
        public string ReportId { set; get; }
        /// <summary>
        /// 查重百分比
        /// </summary>
        public double Percent { set; get; }

    }
}
