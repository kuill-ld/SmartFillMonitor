using System;
using System.Collections.Generic;
using System.Text;
using FreeSql.DataAnnotations;
namespace SmartFillMonitor.Models
{
    [Table(Name = "SystemLog",DisableSyncStructure = true)]
    public class SystemLog
    {
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        [Column(Name = "Timestamp")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// 日志级别
        /// </summary>
        [Column(Name= "Level",StringLength =50)]
        public string  Level { get; set; }
        /// <summary>
        /// 报错信息
        /// </summary>
        [Column(Name= "Exception", StringLength =2000)]
        public string  Exception { get; set; }
        /// <summary>
        /// 输出的日志信息
        /// </summary>
        [Column(Name= "RenderedMessage", StringLength =50)]
        public string  RenderedMessage { get; set; }
        /// <summary>
        /// 线程Id
        /// </summary>
        [Column(Name= "Properties", StringLength =1000)]
        public string  Properties { get; set; }
    }
}
