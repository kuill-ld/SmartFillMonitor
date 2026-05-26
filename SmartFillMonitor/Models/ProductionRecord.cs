using System;
using System.Collections.Generic;
using System.Text;
using FreeSql.DataAnnotations;
namespace SmartFillMonitor.Models
{
    [Table(Name = "ProductionRecord")]
    public class ProductionRecord
    {

        [Column(IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }//主键

        public DateTime Time { get; set; } = DateTime.Now;//记录时间
        [Column(StringLength = 50)]
        public string ?BatchNo { get; set; }//批次号
        public double SettingTemp { get; set; }//设定温度
        public double ActualTemp { get; set; }//实际温度
        public double FillWeight { get; set; }//罐装重量
        public int ActualCount { get; set; }//当前累计产量
        public int TargetCount { get; set; }//当产量
        public bool isNG{ get; set; }//是否NG
        [Column(StringLength = 100)]
        public string ?NgReason { get; set; }//NG原因
        public double CycleTime { get; set; }//产品花费时间
        [Column(StringLength = 50)]
        public string? Operator { get; set; }//操作员


    }
}
