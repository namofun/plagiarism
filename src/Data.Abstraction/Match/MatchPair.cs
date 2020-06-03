namespace SatelliteSite.Data.Match
{
    /// <summary>
    /// 代码段匹配记录
    /// </summary>
    public class MatchPair
    {
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
        public int ContentStartA { set; get; }

        /// <summary>
        /// 文件A的字符中止地址
        /// </summary>
        public int ContentEndA { set; get; }

        /// <summary>
        /// 文件B的字符起始地址
        /// </summary>
        public int ContentStartB { set; get; }

        /// <summary>
        /// 文件B的字符终止地址
        /// </summary>
        public int ContentEndB { set; get; }
    }
}
