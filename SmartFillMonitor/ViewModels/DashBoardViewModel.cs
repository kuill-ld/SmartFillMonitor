using CommunityToolkit.Mvvm.ComponentModel;
using SmartFillMonitor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using LiveCharts.Wpf;
using LiveCharts;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Media;
using SmartFillMonitor.Services;
using System.Windows;
namespace SmartFillMonitor.ViewModels
{
   public partial class DashBoardViewModel:ObservableObject
    {
        [ObservableProperty]
        private int _actualCount;
        [ObservableProperty]
        private int _targetCount;
        [ObservableProperty]
        private double _currentTemp;
        [ObservableProperty]
        private double _settingTemp;
        [ObservableProperty]
        private double _runningTime;
        [ObservableProperty]
        private double _currentCycleTime;
        [ObservableProperty]
        private double _standardCycleTime;
        [ObservableProperty]
        private double _liquidLevel;
        [ObservableProperty]
        private bool _valueOpen=true;
        [ObservableProperty]
        private SeriesCollection _tempLiveCharts;
        [ObservableProperty]
        private string _deviceStatus="自动运行";
        [ObservableProperty]
        private LightState _indicatorState =LightState.Off;
        private string _lastBarCode=string.Empty;

        public ObservableCollection<AlarmUiModel> RecentAlarms { get; } = new();
        public DashBoardViewModel()
        {
            PLCService.DataReceived += OnDataReceived;
            AlarmServcies.AlarmTriggered+= OnAlarmTriggered;    
            _tempLiveCharts = new SeriesCollection()
            {
                new ColumnSeries
                {
                    Title ="温度趋势",
                    Values = new LiveCharts.ChartValues<double>(),
                    Fill=Brushes.Blue,
                    Stroke=Brushes.Blue,
                    StrokeThickness = 1,
                }
            };
           
        }

        private void OnAlarmTriggered(object? sender, AlarmRecord e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                RecentAlarms.Insert(0, AlarmUiModel.FormRecord(e));
                if(RecentAlarms.Count > 10) RecentAlarms.RemoveAt(RecentAlarms.Count - 1);
            }));
        }

        private void OnDataReceived(object? sender, DeviceState e)
        {
            if(e == null) return;
            _ = Task.Run(async () =>
            {
                ActualCount= e.ActualCount;
                TargetCount = e.TargetCount;
                CurrentTemp = e.CurrentTemp;
                SettingTemp = e.SettingTemp;
                RunningTime = e.RunningTime;
                CurrentCycleTime = e.CurrentCycleTime;
                StandardCycleTime = e.StandardCycleTime;
                LiquidLevel = e.LiquidLevel;
                ValueOpen= e.ValueOpen;
                var barcode= e.BarCode ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(barcode) && barcode != _lastBarCode)
                {
                    _lastBarCode = barcode;
                    LogService.Info($"检测到新条码: {barcode}");
                    var record = new ProductionRecord
                    {
                      Time = DateTime.Now,
                      BatchNo = barcode,
                      SettingTemp = e.SettingTemp,
                      CycleTime = e.CurrentCycleTime,
                      ActualTemp = e.CurrentTemp,
                      TargetCount = e.TargetCount,
                      ActualCount = e.ActualCount,
                      Operator = "",
                      isNG = false,
                    };
                    await DbProvider.Fsql.Insert(record).ExecuteAffrowsAsync();
                }
            });
            
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if(TempLiveCharts!=null && TempLiveCharts.Count > 0)
                {
                    TempLiveCharts[0].Values.Add(e.CurrentTemp);
                    if (TempLiveCharts[0].Values.Count > 40)
                    {
                        TempLiveCharts[0].Values.RemoveAt(0);
                    }
                }
                   
            }));
        }

        [RelayCommand]
        private async Task StartProductionAsync()
        {
            try
            {
                DeviceStatus = "启动中";
                IndicatorState = LightState.Green;
                await PLCService.WriteCommandAsync("Start",true);
                await Task.Delay(500);
                DeviceStatus = "运行中";
                LogService.Info("生产启动命令已发送");
            }
            catch (Exception ex)
            {
                DeviceStatus = "启动失败";
                IndicatorState = LightState.Red;
                LogService.Error("生产启动命令发送失败",ex);
            }
        }
        [RelayCommand]
        private async Task StopProductionAsync()
        {
            try
            {
                DeviceStatus = "停止中";
                IndicatorState = LightState.Red;
                await PLCService.WriteCommandAsync("Stop", true);
                await Task.Delay(500);
                DeviceStatus = "已停止";
                LogService.Info("生产停止命令已发送");
            }
            catch (Exception ex)
            {
                DeviceStatus = "停止失败";
                IndicatorState = LightState.Red;
                LogService.Error("生产停止命令发送失败", ex);
            }
        }
        [RelayCommand]
        private async Task ResetProductionAsync()
        {
            try
            {
                DeviceStatus = "复位中";
                IndicatorState = LightState.Yellow;
                await PLCService.WriteCommandAsync("Stop", true);
                await Task.Delay(2000);
                await PLCService.WriteCommandAsync("Reset", true);
                DeviceStatus = "已就绪";
                IndicatorState = LightState.Off;
                LogService.Info("发送复位脉冲");
            }
            catch (Exception ex)
            {
                DeviceStatus = "复位失败";
                IndicatorState = LightState.Red;
                LogService.Error("复位命令发送失败", ex);
            }
        }

    }
}
