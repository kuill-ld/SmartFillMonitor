using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SmartFillMonitor.Models;
using SmartFillMonitor.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace SmartFillMonitor.ViewModels
{
   public partial class DashQueryViewModel:ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<ProductionRecord> _records = new();
        [ObservableProperty]
        private ProductionRecord? _selectedRecord;
        [ObservableProperty]
        private DateTime? _startDate = DateTime.Now.AddDays(-7);
        [ObservableProperty]
        private DateTime? _endDate = DateTime.Today;
        public DashQueryViewModel()
        {
            
        }
        [RelayCommand]
        private async Task QueryAsync()
        {
            var start = StartDate ?? DateTime.Today.AddDays(-7);
            var end = EndDate ?? DateTime.Today;
            var endInclusive = end.AddDays(1).AddMicroseconds(-1);
            var list = await DataService.QueryRecordAsync(start, endInclusive);
            Records.Clear();
            foreach (var record in list.OrderByDescending(r=>r.Time))
            {
                Records.Add(record);
            }
        }
        [RelayCommand]
        private async Task ExportAsync()
        {
            if (Records == null || Records.Count == 0) return;
            
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"ProductionRecords_{DateTime.Now:yyyyMMddHHmmss}.csv"
            };
            if(saveFileDialog.ShowDialog() != true) return;
            try
            {
                await DataService.ExportToAsync(Records.ToList(), saveFileDialog.FileName);
            }
            catch (Exception ex)
            {
                LogService.Error($"导出生产数据失败", ex);
            }
        } 
    }
}
