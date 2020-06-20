using System;
using System.Collections.Generic;

namespace SatelliteSite.Data
{
    /// <summary>
    /// 表示一份提交的代码
    /// </summary>
    public class Submission
    {
        /// <summary>
        /// 代码的唯一编号
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 学生姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 最高重复率
        /// </summary>
        public double MaxPercent { get; set; }

        /// <summary>
        /// 是否已生成比较单元
        /// </summary>
        public bool TokenProduced { get; set; }

        /// <summary>
        /// 提交时间
        /// </summary>
        public DateTimeOffset UploadTime { get; set; }

        /// <summary>
        /// 代码提交中的所有文件
        /// </summary>
        public ICollection<SubmissionFile> Files { get; set; }
        
        /// <summary>
        /// 代码的所有Token
        /// </summary>
        public ICollection<Token> Tokens { get; set; }
        
        /// <summary>
        /// 代码使用的语言
        /// </summary>
        public string Language { get; set; }
    }
}
