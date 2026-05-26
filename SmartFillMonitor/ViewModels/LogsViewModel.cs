using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartFillMonitor.Models;
using SmartFillMonitor.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;

namespace SmartFillMonitor.ViewModels
{
    public partial class LogsViewModel : ObservableObject
    {
        [ObservableProperty]
        private DateTime _startDate = DateTime.Today;
        [ObservableProperty]
        private DateTime _endDate = DateTime.Today.AddDays(1).AddSeconds(-1);
        [ObservableProperty]
        private string _selectedLevel = "All";
        [ObservableProperty]
        private bool _isBusy;
        [ObservableProperty]
        private int _totalCount;
        [ObservableProperty]
        private int _pageIndex = 1;

        private const int pageSize = 2;//每页显示50条数据


        public ObservableCollection<string> LogLevels { get; } = new()
        {
            "All", "Debug", "Information", "Warning", "Error"
        };
        [ObservableProperty]
        private ObservableCollection<SystemLog> _logs = new();

        [ObservableProperty]
        private string _searchText = "";
        [RelayCommand]
        private async Task SearchAsync()
        {
            PageIndex = 1;
            await LoadLogsAsync();
        }
        [RelayCommand]
        private async Task ExportAsync()
        {
            var query = BuildQuery();
            var allData = await query.OrderByDescending(x => x.Timestamp).ToListAsync();
            if (allData.Count == 0)
            {
                MessageBox.Show("无数据导出");//后面改为Messager去解耦合
                return;

            }
            var line = new List<string> { "时间,等级,内容,异常" };
            line.AddRange(allData.Select
                ( x =>$"{x.Timestamp:yyyy-MM-dd HH:mm:ss},\"{x.Level?.Replace("\"", "\"\"") ?? ""}\",\"{x.RenderedMessage?.Replace("\"", "\"\"") ?? ""}\",\"{x.Exception?.Replace("\"", "\"\"").Replace("\n", "").Replace("\r", "") ?? ""}\""
                ));
            var path = $"LogsExport_{DateTime.Now:yyyyMMddHHmmss}.csv";
            await File.WriteAllLinesAsync(path, line,Encoding.UTF8);
            MessageBox.Show($"日志导入到文件:{Path.GetFullPath(path)}");




        }
        [RelayCommand]
        private async Task PrePageAsync()
        {
            if (PageIndex <= 1) return;
            PageIndex--;
            await LoadLogsAsync();
        }
        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (PageIndex * pageSize >= TotalCount) return;
            PageIndex++;
            await LoadLogsAsync();
        }
        private async Task LoadLogsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var query = BuildQuery();
                TotalCount = (int)await query.CountAsync();
                var data = await query.OrderByDescending(x => x.Timestamp).Page(PageIndex, pageSize).ToListAsync();
                Logs = new ObservableCollection<SystemLog>(data);
            }
            catch (Exception ex)
            {
                LogService.Warn($"查询失败 :{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
        public LogsViewModel()
        {
            _ = LoadLogsAsync();
        }
        private FreeSql.ISelect<SystemLog> BuildQuery()
        {
            var query = DbProvider.Fsql.Select<SystemLog>();
            var start = StartDate.ToString("yyyy-MM-dd");
            var end = EndDate.ToString("yyyy-MM-dd ");
            query = query.Where($"date(\"Timestamp\") >=date('{start}')AND date(\"Timestamp\") <=date('{end}')");
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(x => x.RenderedMessage.Contains(SearchText));
            }
            if (SelectedLevel != "All" && !string.IsNullOrEmpty(SelectedLevel))
            {
                query = query.Where(x => x.Level.Contains(SelectedLevel));
            }

            return query;
        }



    }
}
