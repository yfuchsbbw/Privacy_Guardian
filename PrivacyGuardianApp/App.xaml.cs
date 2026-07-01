using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PrivacyGuardian.Core;
using PrivacyGuardian.Database;
using PrivacyGuardian.Services;
using PrivacyGuardian.ViewModels;
using PrivacyGuardian.Views;

namespace PrivacyGuardian;

public partial class App : System.Windows.Application
{
    private IHost? _host;
    private bool _startInBackground;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _startInBackground = e.Args.Any(argument =>
            argument.Equals("--background", StringComparison.OrdinalIgnoreCase) ||
            argument.Equals("/background", StringComparison.OrdinalIgnoreCase) ||
            argument.Equals("--silent", StringComparison.OrdinalIgnoreCase));

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<IUiDispatcher, WpfUiDispatcher>();
                services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();
                services.AddSingleton<ILogRepository, SqliteLogRepository>();
                services.AddSingleton<ISystemMetricsService, SystemMetricsService>();
                services.AddSingleton<IProcessService, ProcessService>();
                services.AddSingleton<INetworkService, NetworkService>();
                services.AddSingleton<IPrivacyMonitorService, PrivacyMonitorService>();
                services.AddSingleton<IStartupService, StartupService>();
                services.AddSingleton<IFileActivityService, FileActivityService>();
                services.AddSingleton<IUsbMonitorService, UsbMonitorService>();
                services.AddSingleton<IFirewallService, FirewallService>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<INotificationService, NotificationService>();
                services.AddSingleton<ITrayService, TrayService>();
                services.AddSingleton<IThemeService, ThemeService>();
                services.AddSingleton<ILocalizationService, LocalizationService>();
                services.AddSingleton<BackgroundMonitorService>();
                services.AddSingleton<IBackgroundMonitorService>(provider => provider.GetRequiredService<BackgroundMonitorService>());
                services.AddHostedService(provider => provider.GetRequiredService<BackgroundMonitorService>());
                services.AddSingleton<UpdateService>();
                services.AddSingleton<IUpdateService>(provider => provider.GetRequiredService<UpdateService>());
                services.AddHostedService(provider => provider.GetRequiredService<UpdateService>());

                services.AddSingleton<MainViewModel>();
                services.AddSingleton<DashboardViewModel>();
                services.AddSingleton<ProcessesViewModel>();
                services.AddSingleton<NetworkViewModel>();
                services.AddSingleton<PrivacyViewModel>();
                services.AddSingleton<StartupViewModel>();
                services.AddSingleton<FileActivityViewModel>();
                services.AddSingleton<UsbViewModel>();
                services.AddSingleton<FirewallViewModel>();
                services.AddSingleton<LogsViewModel>();
                services.AddSingleton<SettingsViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();

        await _host.StartAsync();
        await _host.Services.GetRequiredService<IDatabaseInitializer>().InitializeAsync();
        var settings = _host.Services.GetRequiredService<ISettingsService>();
        await settings.LoadAsync(CancellationToken.None);
        _host.Services.GetRequiredService<IThemeService>().Apply(settings.Current.IsDarkMode);
        _host.Services.GetRequiredService<ILocalizationService>().SetLanguage(settings.Current.Language);

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _host.Services.GetRequiredService<MainViewModel>();
        if (_startInBackground)
        {
            mainWindow.Show();
            mainWindow.Hide();
            _host.Services.GetRequiredService<ITrayService>().HideMainWindow();
        }
        else
        {
            mainWindow.Show();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(3));
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
