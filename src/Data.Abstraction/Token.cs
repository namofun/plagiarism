namespace SatelliteSite.Data
{
    /// <summary>
    /// 代码中的语法符号
    /// </summary>
    public class Token
    {
        /// <summary>
        /// 语法符号类型（与语言相关）
        /// </summary>
        public int Type { get;  set; }

        /// <summary>
        /// 对应文件行号
        /// </summary>
        public int Line { get;  set; }

        /// <summary>
        /// 对应文件号
        /// </summary>
        public int FileId { get; set; }

        /// <summary>
        /// 对应文件的字符地址
        /// </summary>
        public int Column { get;  set; }

        /// <summary>
        /// 语法符号长度
        /// </summary>
        public int Length { get;  set; }

        /// <summary>
        /// 令牌号
        /// </summary>
        public int TokenId { get; set; }

        /// <summary>
        /// 将 <see cref="Plag.Token"/> 转换为 <see cref="Token"/>。
        /// </summary>
        /// <param name="token">原先的符号</param>
        public static implicit operator Token(Plag.Token token)
        {
            return new Token
            {
                Column = token.Column,
                Length = token.Length,
                Line = token.Line,
                Type = token.Type,
                FileId = token.FileId
            };
        }
    }
}
