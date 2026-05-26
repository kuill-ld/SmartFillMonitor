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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SmartFillMonitor.UserControls
{
    /// <summary>
    /// StatusIndicator.xaml 的交互逻辑
    /// </summary>
    public partial class StatusIndicator : UserControl
    {
        public StatusIndicator()
        {
            InitializeComponent();
        }


        public bool IsOnline
        {
            get { return (bool)GetValue(IsOnlineProperty); }
            set { SetValue(IsOnlineProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsOnline.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsOnlineProperty =
            DependencyProperty.Register(nameof(IsOnline), typeof(bool), typeof(StatusIndicator), new PropertyMetadata(false));


        public bool IsWarning
        {
            get { return (bool)GetValue(IsWarningProperty); }
            set { SetValue(IsWarningProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsWarning.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsWarningProperty =
            DependencyProperty.Register(nameof(IsWarning), typeof(bool), typeof(StatusIndicator), new PropertyMetadata(false));


        public string StatusText
        {
            get { return (string)GetValue(StatusTextProperty); }
            set { SetValue(StatusTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StatusText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register(nameof(StatusText), typeof(string), typeof(StatusIndicator), new PropertyMetadata("状态"));




    }
}
