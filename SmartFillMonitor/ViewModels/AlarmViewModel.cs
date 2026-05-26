using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartFillMonitor.Models;
using SmartFillMonitor.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace SmartFillMonitor.ViewModels
{
    public partial class AlarmViewModel : ObservableObject
    {
        /// <summary>
        /// 当前活跃的报警列表
        /// </summary>
        public ObservableCollection<AlarmUiModel> ActiveAlarms { get; set; }
        /// <summary>
        /// 历史报警列表
        /// </summary>
        public ObservableCollection<AlarmUiModel> HistroyAlarms { get; set; }
        [ObservableProperty]
        private int _activeAlarmCount;
        public AlarmViewModel()
        {
            ActiveAlarms = new();
            HistroyAlarms = new();
            AlarmServcies.AlarmTriggered += OnAlarmTriggered;
            _ = LoadActiveAlarmsAsync();
        }
        /// <summary>
        /// 测试报警
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task TestAlarm()
        {
            await AlarmServcies.TriggerAlarmAsync(new Models.AlarmRecord
            {
                AlarmCord = Models.AlarmCord.CommunicationError,
                Message = "测试报警触发",
                StartTime = DateTime.Now,
                IsActive = true,
                Description = "这是一个测试报警，模拟压力过低的情况，请检查设备是否正常工作。",
                AlarmSeverity = Models.AlarmSeverity.General
            });
        }
       
        /// <summary>
        /// 加载活跃的报警
        /// </summary>
        /// <returns></returns>
        private async Task LoadActiveAlarmsAsync()
        {
            try
            {
                var records = await AlarmServcies.GetActiveAlarmsAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ActiveAlarms.Clear();
                    foreach (var item in records)
                    {
                        ActiveAlarms.Add(AlarmUiModel.FormRecord(item));
                    }
                    ActiveAlarmCount = ActiveAlarms.Count;
                });
            }
            catch (Exception e)
            {
                LogService.Error($"加载活跃报警失败:{e.Message}");
            }
        }

        private void OnAlarmTriggered(object? sender, AlarmRecord e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var alarm = AlarmUiModel.FormRecord(e);
                ActiveAlarms.Insert(0, alarm);
                ActiveAlarmCount = ActiveAlarms.Count;
                LogService.Error($"新报警:{alarm.Code}:{alarm.Title}");
            });
        }

        /// <summary>
        /// 刷新界面
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task Refresh()
        {
            await LoadActiveAlarmsAsync();
        } 
        /// 是否复位报警
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task AckConfirm(AlarmUiModel alarmUiModel)
        {
            if (alarmUiModel == null) return;
            try
            {
                var suceess = await AlarmServcies.AckAlarmAsync(alarmUiModel.Id, "");
                if(suceess)
                {
                    ActiveAlarms.Remove(alarmUiModel);
                    ActiveAlarmCount = ActiveAlarms.Count;
                    LogService.Info($"确认报警:ID:{alarmUiModel.Id}");
                }
                else
                {
                    LogService.Warn($"确认报警:{alarmUiModel.Id}失败，可能已经被确认了");
                };
            }
            catch (Exception e)
            {
                LogService.Error($"确认报警异常:{alarmUiModel.Id}",e);
            }
        }
        /// <summary>
        /// 历史查询
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task LoadHistory()
        {
            var record = await AlarmServcies.GetHistoryAlarmsAsync(1,5,HistoryStartDate, HistoryEndDate,AlarmSeverity.All);
            Application.Current.Dispatcher.Invoke(() =>
            {
                HistroyAlarms.Clear();
                foreach (var item in record.Items)
                {
                    HistroyAlarms.Add(AlarmUiModel.FormRecord(item));
                }
            });
        }
        [ObservableProperty]
        private DateTime _historyStartDate = DateTime.Today.AddDays(-1);
        [ObservableProperty]
        private DateTime _historyEndDate = DateTime.Today;



    }
}
