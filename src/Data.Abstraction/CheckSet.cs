using System;

namespace SatelliteSite.Data
{
    /// <summary>
    /// 查重集
    /// </summary>
    public class CheckSet
    {
        /// <summary>
        /// 查重集编号
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreateTime { get; set; }

        /// <summary>
        /// 查重集名称
        /// </summary>
        public string Name { get; set; }
    }
}
