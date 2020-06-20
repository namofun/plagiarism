namespace SatelliteSite.Data
{
    /// <summary>
    /// 代码中的文件
    /// </summary>
    public class SubmissionFile
    {
        /// <summary>
        /// 文件编号
        /// </summary>
        public int FileId { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }
        
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件内容
        /// </summary>
        public string Content { get; set; }
    }
}
