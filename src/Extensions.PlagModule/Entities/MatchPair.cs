namespace SatelliteSite.Entities
{
    /// <summary>
    /// 代码段匹配记录
    /// </summary>
    public class MatchPair
    {
        /// <summary>
        /// MatchPair编号
        /// </summary>
        public int MatchingId { get; set; }

        /// <summary>
        /// 文件A编号
        /// </summary>
        public int FileA { get; set; }

        /// <summary>
        /// 文件B编号
        /// </summary>
        public int FileB { get; set; }

        /// <summary>
        /// 文件A的字符起始地址
        /// </summary>
        public int ContentStartA { get; set; }

        /// <summary>
        /// 文件A的字符中止地址
        /// </summary>
        public int ContentEndA { get; set; }

        /// <summary>
        /// 文件B的字符起始地址
        /// </summary>
        public int ContentStartB { get; set; }

        /// <summary>
        /// 文件B的字符终止地址
        /// </summary>
        public int ContentEndB { get; set; }
    }
}
