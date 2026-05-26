using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using FreeSql.DataAnnotations;
namespace SmartFillMonitor.Models
{
    #region 数据库实体模型
    /// <summary>
    /// 数据库实体模型
    /// </summary>
    [Table(Name = "AlarmRecord")]
    public class AlarmRecord
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        /// <summary>
        /// 报警类型
        /// </summary>
        public AlarmCord AlarmCord { get; set; }
        /// <summary>
        /// 报警级别
        /// </summary>
        public AlarmSeverity AlarmSeverity { get; set; }
        /// <summary>
        /// 报警开始时间
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 设备解除故障时间
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 报警持续时间
        /// </summary>
        public double? Duration { get; set; }
        /// <summary>
        /// 报警是否为活跃状态，活跃状态表示设备当前仍处于报警状态，非活跃状态表示设备已经解除报警
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// 报警是否人工确认
        /// </summary>
        public bool IsRead { get; set; }
        /// <summary>
        /// 确认的时间
        /// </summary>
        public DateTime? ReadTime { get; set; }
        /// <summary>
        /// 确认的操作人
        /// </summary>
        public string? RemarkUser { get; set; }
        /// <summary>
        /// 动态消息（温度过高）
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// 处理建议
        /// </summary>
        public string? Description { get; set; }
    }

    public enum AlarmSeverity
    {
        [Description("所有")]
        All = 0,
        [Description("一般")]
        General = 1,
        [Description("重要")]
        Important = 2,
        [Description("紧急")]
        Urgent = 3
    }
    public enum AlarmCord
    {
        [Description("正常")]
        Normal = 0,
        [Description("原料桶液位过低")]
        LowLiquidLevel = 1001,
        [Description("压力偏低")]
        LowPressure = 2001,
        [Description("温度过高")]
        HighTemperature = 3001,
        [Description("PLC通信故障")]
        CommunicationError = 4001,
        [Description("系统报警")]
        SystemError = 5001,


    }
    #endregion 

    #region UI视图模型
    /// <summary>
    /// UI视图模型
    /// </summary>
    public class AlarmUiModel : INotifyPropertyChanged
    {
        private long _id;
        private string _code;
        private string _title;
        private string _timeStr;
        private string _description;

        public long Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Code
        {
            get => _code;
            set
            {
                if (_code != value)
                {
                    _code = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }
        public string TimeStr
        {
            get => _timeStr;
            set
            {
                if (_timeStr != value)
                {
                    _timeStr = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string v = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 数据库实体模型转换为UI视图模型
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static AlarmUiModel FormRecord(AlarmRecord record)
        {
            var title = record.AlarmCord.GetDescription();
            return new AlarmUiModel
            {
               Id = record.Id,
               Code =$"E{(int)record.AlarmCord}",
               Title = title,
               Description = record.Description,
               TimeStr = record.StartTime.ToString("MM-dd HH:mm:ss")
            };
        }
    }
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field.GetCustomAttribute(typeof(DescriptionAttribute), false) as DescriptionAttribute;
            return attribute != null? attribute.Description : value.ToString();
        }
    }
    #endregion

}
