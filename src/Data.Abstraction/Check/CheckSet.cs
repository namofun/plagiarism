using System.Collections.Generic;
using System;
using Newtonsoft.Json;

namespace SatelliteSite.Data.Check
{
    /// <summary>
    /// CheckSet 一份报告
    /// </summary>
    public class CheckSet
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        /// <summary>
        /// TeamReport集合
        /// </summary>
        public ICollection<TeamReport> Teams { set; get; }
        /// <summary>
        /// 生成时间
        /// </summary>
        public DateTimeOffset CreateTime { set; get; }
    }
}
