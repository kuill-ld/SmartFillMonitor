using Microsoft.Extensions.DependencyInjection;
using SmartFillMonitor.Models;
using SmartFillMonitor.Services;
using SmartFillMonitor.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SmartFillMonitor.Views
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                await LoadUSerAsync();
                if(PasswordBox != null)
                {
                    PasswordBox.Focus();// 设置焦点到密码输入框
                }
            };
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    LoginClick(this, new RoutedEventArgs());
                }
                if(e.Key == Key.Escape)
                {
                    Cancel_Click(this, new RoutedEventArgs());
                    e.Handled = true; // 阻止事件继续传播
                }
            };
        }

        private async Task LoadUSerAsync()
        {
            try
            {
                List<User> ?users = await UserService.GetAllUsersAsync();
                UserNameCombo.ItemsSource = users;
                if(users != null && users.Count > 0)
                {
                    UserNameCombo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {   

            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private async void LoginClick(object sender, RoutedEventArgs e)
        {
            var user = (UserNameCombo.SelectedValue as string)?? string.Empty ;
            var password = PasswordBox.Password ?? string.Empty;
            if(string.IsNullOrEmpty(user) )
            {
                MessageBox.Show("请输入用户名","提示",MessageBoxButton.OK,MessageBoxImage.Information);
                UserNameCombo.Focus();
                return;
            }
            IsEnabled= true;// 禁用界面，防止重复点击
            try
            {
                var ok= await UserService.AuthernticateAsync(user,password);
                if (ok)
                {
                    DialogResult = true;
                    Close();

                }
                else
                {
                    MessageBox.Show("用户名或密码错误", "登录失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                   PasswordBox.Clear();    
                    PasswordBox.Focus();
                }
            }
            finally
            {
                IsEnabled= true;// 恢复界面状态
            }
        }
     
        
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult= false;
            Close();
        }
    }
}
