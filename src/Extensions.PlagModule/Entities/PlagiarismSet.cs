using System;
using System.Collections.Generic;

namespace SatelliteSite.Entities
{
    /// <summary>
    /// 查重集
    /// </summary>
    public class PlagiarismSet
    {
        /// <summary>
        /// 查重集编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreateTime { get; set; }

        /// <summary>
        /// 查重集名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 报告数量
        /// </summary>
        public int ReportCount { get; set; }

        /// <summary>
        /// 等待处理的报告数量
        /// </summary>
        public int ReportPending { get; set; }

        /// <summary>
        /// 所有提交
        /// </summary>
        public ICollection<PlagiarismSubmission> Submissions { get; set; }
    }
}
