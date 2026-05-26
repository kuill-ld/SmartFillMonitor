using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFillMonitor.Models
{
    public class DeviceSettings
    {
        //串口设置
        public string PortName { get; set; } = "COM1";
        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;
        public string Parity { get; set; } = "None";
        public string StopBits { get; set; } = "One";
        //系统设置
        public bool IsAutoLogin { get; set; } = true;
        public bool IsAlarmSound { get; set; } = true;
        public bool IsDebugModel { get; set; } = true;

    }
}
