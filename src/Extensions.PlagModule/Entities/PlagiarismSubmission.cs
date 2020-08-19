using System;
using System.Collections.Generic;

namespace SatelliteSite.Entities
{
    /// <summary>
    /// 表示一份提交的代码
    /// </summary>
    public class PlagiarismSubmission
    {
        /// <summary>
        /// 代码的唯一编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 隶属查重集编号
        /// </summary>
        public int SetId { get; set; }

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
        public bool? TokenProduced { get; set; }

        /// <summary>
        /// 提交时间
        /// </summary>
        public DateTimeOffset UploadTime { get; set; }

        /// <summary>
        /// 代码提交中的所有文件
        /// </summary>
        public ICollection<PlagiarismFile> Files { get; set; }
        
        /// <summary>
        /// 代码的所有Token
        /// </summary>
        public Plag.Submission Tokens { get; set; }
        
        /// <summary>
        /// 代码使用的语言
        /// </summary>
        public string Language { get; set; }
    }
}
