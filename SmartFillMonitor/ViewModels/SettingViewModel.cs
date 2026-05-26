using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartFillMonitor.Models;
using SmartFillMonitor.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Text;

namespace SmartFillMonitor.ViewModels
{
    public partial class SettingViewModel:ObservableObject
    {
        public ObservableCollection<string> PortNames { get; } = new();
        public ObservableCollection<int> BaudRates { get; } = new() { 9600,19200,38400,57600,115200};
        public ObservableCollection<string> ParityBits { get; } = new() { "None","Odd","Even","Mark","Space"};
        public ObservableCollection<int> DataBits { get; } = new() { 7, 8 };
        public ObservableCollection<string> StopBits { get; } = new() { "None", "One", "Two" };
        [ObservableProperty]
        private string _selectedPortName ="COM3";
        [ObservableProperty]
        private int _selectedBaudRate=9600;
        [ObservableProperty]
        private int _selectedDataBit=8;
        [ObservableProperty]
        private string _selectedStopBit = "None";
        [ObservableProperty]
        private string _selectedParityBit="None";
        [ObservableProperty]
        private bool _isAutoLogin = true;
        [ObservableProperty]
        private bool _isAlarmSound = true;
        [ObservableProperty]
        private bool _isDebugModel = true;

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                var model = new DeviceSettings
                {
                    PortName = SelectedPortName,
                    Parity = SelectedParityBit,
                    DataBits = SelectedDataBit,
                    BaudRate = SelectedBaudRate,
                    StopBits = SelectedStopBit,
                    IsDebugModel = IsDebugModel,
                    IsAutoLogin = IsAutoLogin,
                    IsAlarmSound = IsAlarmSound,

                };
                await ConfigServices.SaveConfigAsync(model);
            }
            catch (Exception e)
            {
                LogService.Error($"保存配置失败：{e.Message}");
               
            }
        }
        public  SettingViewModel()
        {
            RefreshPortNames();
            try
            {
              _ = LoadSettings();//初始打开加载配置
            }
            catch (Exception e)
            {
                LogService.Error($"加载配置失败，将使用默认设置：{e.Message}");
                //加载失败，使用默认设置（以及在LoadSettings里面做了处理不需要额外处理）
            }
        }

        private async Task LoadSettings()
        {
            try
            {
                var ds = await ConfigServices.LoadConfigAsync();
                SelectedPortName = ds.PortName;
                SelectedBaudRate = ds.BaudRate;
                SelectedDataBit = ds.DataBits;
                SelectedParityBit = ds.Parity;
                SelectedStopBit = ds.StopBits;
                IsAlarmSound = ds.IsAlarmSound;
                IsAutoLogin = ds.IsAutoLogin;
                IsDebugModel = ds.IsDebugModel;
            }
            catch (Exception e)
            {
                LogService.Error( $"加载配置失败，将使用默认设置:{e.Message}");
            }
           
          
        }
        private void RefreshPortNames()
        {
            PortNames.Clear();
            try
            {
               
                var ports = PLCService.GetPortNames()?? SerialPort.GetPortNames();
                foreach (var port in ports)
                {
                    PortNames.Add(port);
                }
                //如果当前选中的串口不在列表中，默认选中第一个
                if (!string.IsNullOrEmpty(SelectedPortName) && !PortNames.Contains(SelectedPortName))
                {
                    SelectedPortName = PortNames.Count > 0 ? PortNames[0]:SelectedPortName;
                }
            }
            catch (Exception e)
            {
                LogService.Warn ($"获取串口列表失败:{e.Message}");
                PortNames.Clear();
                PortNames.Add("COM1");
                PortNames.Add("COM2");
                PortNames.Add("COM3");
                PortNames.Add("COM4");

            }
          
        }
    }
}
