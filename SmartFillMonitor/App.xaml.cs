using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SmartFillMonitor.ViewModels;
using SmartFillMonitor.Views;
using System.Configuration;
using System.Data;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using SmartFillMonitor.Services;

namespace SmartFillMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static RichTextBox LogView = new RichTextBox()
        {

            IsReadOnly = true,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Background = Brushes.Black,
            Foreground = Brushes.White,
            FontSize = 20
        };
        private const string LogTemplate = "[{Timestamp: yyyy-mm-dd HH:mm:ss fff} {Level:u3}] {Message:lj}{NewLine}{Exception}";
        public static IServiceProvider ServiceProvider { get; private set; }
        private const string LogPath = "Logs\\log-.txt";
        private const string SqlitePath = "SmartFill.db";
        private const string SqliteConnStr = "Data Source=SmartFill.db;";
        protected override async void OnStartup(StartupEventArgs e)
        {
            Console.WriteLine("ggg");
            base.OnStartup(e);
            SetExceptionHanding();//设置全局异常
            ConfigLog();//配置日志
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
            await InitialCoreServicesAsync();
            await InitialLoginFolowAsync(); 
            LogService.Debug("初始化PLC Services");

            var plcSetttings = await ConfigServices.LoadConfigAsync();
            await PLCService.Initialize(plcSetttings);
        }

        private void SetExceptionHanding()
        {
            //UI线程异常
            DispatcherUnhandledException += (s, e) =>
            {
                LogService.Error($"发生未处理的UI线程异常",e.Exception);
                MessageBox.Show($"发生未处理的异常: {e.Exception.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true; // 阻止应用程序崩溃
            };
            //非UI线程异常
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                LogService.Fatal($"发生未处理的非UI线程异常", ex);
            };
            //Task内部异常
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogService.Fatal($"发生未观察到的Task异常", e.Exception);
                e.SetObserved(); // 阻止应用程序崩溃，标记为已处理
            };

        }

        private async Task InitialLoginFolowAsync()
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var loginWindow = new LoginWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            bool? result = loginWindow.ShowDialog();    
            if(result == true)
            {
                LogService.Info("用户登录成功，正在加载主界面...");
                var mainVm = ServiceProvider.GetRequiredService<MainWindowViewModel>();
                var mainWindow = new MainWindow()
                {
                    DataContext = mainVm
                };
                Current.MainWindow = mainWindow;
                ShutdownMode = ShutdownMode.OnMainWindowClose;
                mainWindow.Show();
            }
            else
            {
                // 用户关闭或取消登录，退出应用避免后台残留
                Current.Shutdown();
            }
        }
        private async Task InitialCoreServicesAsync()
        {
            Log.Debug("正在初始化核心服务...");
            DbProvider.Initialize(SqliteConnStr);
            
            LogService.Info($"Core Services Initialized successfully");
            await UserService.InitializeAsync();//确保数据库基础表格结构存在
           
        }

        private void ConfigLog()
        {
           new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithThreadId()
                .WriteTo.RichTextBox(LogView, outputTemplate: LogTemplate)
                .WriteTo.Console(outputTemplate: LogTemplate)
                .WriteTo.File(LogPath, rollingInterval: RollingInterval.Day, outputTemplate: LogTemplate, shared: true)
                .WriteTo.SQLite(SqlitePath,tableName:"SystemLog",storeTimestampInUtc:false)
                .CreateLogger();

        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();//确保日志被正确写入和资源被释放
            base.OnExit(e);
        }
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<AlarmViewModel>();
            services.AddSingleton<DashBoardViewModel>();
            services.AddSingleton<DashQueryViewModel>();
            services.AddSingleton<LogsViewModel>();
            services.AddSingleton<SettingViewModel>();
            services.AddSingleton<MainWindowViewModel>();
         

        }
    }

}
