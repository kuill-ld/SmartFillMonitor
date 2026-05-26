using CommunityToolkit.Mvvm.ComponentModel;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SmartFillMonitor.Models
{
    public class DeviceState
    {
        
        public int ActualCount { get; set; }//当前产量

        public int TargetCount {  get; set; }//总产量

        public double CurrentTemp {  get; set; }//实时温度
     
        public double SettingTemp {  get; set; }//设定温度
       
        public double RunningTime {  get; set; }//运行时间
       
        public double CurrentCycleTime {  get; set; }//当前节拍
        
        public double StandardCycleTime {  get; set; }//总节拍
      
        public double LiquidLevel { get; set; }//当前液位
      
        public bool ValueOpen { get; set; } = true;//当前阀门状态
       
        public string BarCode { get; set; } = string.Empty;//二维码
    }
}
