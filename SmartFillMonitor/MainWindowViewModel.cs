using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SmartFillMonitor.Models;
using SmartFillMonitor.Services;
using SmartFillMonitor.ViewModels;
using SmartFillMonitor.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace SmartFillMonitor
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly DispatcherTimer _timer;
        private IServiceProvider serviceProvider;
        public MainWindowViewModel()
        {
            serviceProvider = App.ServiceProvider;
            PLCService.ConnectionChanged += (x, connected) => IsPlcConnected = connected;
         
            PLCService.DataReceived += PlcService_DataRecived;
            UserService.LoginStateChanged += user =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    UpdateUser(user);
                });
            };
            UpdateUser(UserService.CurrentUser);
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
            MainContent = serviceProvider.GetRequiredService<DashBoardViewModel>();
        }

        private void UpdateUser(User user)
        {
          if(user == null)
            {
                CurrentUserDisplayName = "未登入";
                IsUserLogging  = false; 
                IsAdmin = false;
            }
            else
            {
                CurrentUserDisplayName = user.UserName;
                IsUserLogging = true;
                IsAdmin = user.Role == Role.Admin;
            }
        }

        private void PlcService_DataRecived(object? sender, DeviceState e)
        {
            CurrentBatchNo = e.BarCode;

        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        [ObservableProperty]
        private string _currentTime;
        [ObservableProperty]
        private object _mainContent;
        [ObservableProperty]
        private string _currentBatchNo = "";
        [ObservableProperty]
        private bool _isPlcConnected;
        [ObservableProperty]
        private string _currentUserDisplayName ="未登入";
        [ObservableProperty]
        private bool _isAdmin;
        [ObservableProperty]
        private bool _isUserLogging;
        [ObservableProperty]
        private LightState _indicatorState = LightState.Off;
        [RelayCommand]
        private void Navigate(string para)
        {
            if (string.IsNullOrEmpty(para)) return;
            switch (para)
            {
                case "DashBoard": MainContent = serviceProvider.GetRequiredService<DashBoardViewModel>();break;
                case "DashQuery": MainContent = serviceProvider.GetRequiredService<DashQueryViewModel>();break;
                case "Alarms": MainContent = serviceProvider.GetRequiredService<AlarmViewModel>();break;
                case "Logs": MainContent = serviceProvider.GetRequiredService<LogsViewModel>();break;
                case "Setting": MainContent = serviceProvider.GetRequiredService<SettingViewModel>();break;
                default: break;
            }
            
        }
        [RelayCommand]
        private void Login()
        {
            var loginWin = new LoginWindow()
            {
                Owner = Application.Current.MainWindow

            };
            loginWin.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = loginWin.ShowDialog();
            UpdateUser(UserService.CurrentUser);
        }
        [RelayCommand]
        private void Exit()
        {
            var result = MessageBox.Show("确定要退出系统吗？", "确认", MessageBoxButton.YesNo);
            if(result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }



    }
}
