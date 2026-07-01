using PrivacyGuardian.Core;
using PrivacyGuardian.Models;

namespace PrivacyGuardian.Services;

public interface ISystemMetricsService
{
    Task<SystemMetrics> GetMetricsAsync(CancellationToken cancellationToken);
}

public interface IProcessService
{
    Task<IReadOnlyList<ProcessInfo>> GetProcessesAsync(CancellationToken cancellationToken);
}

public interface INetworkService
{
    Task<IReadOnlyList<NetworkConnection>> GetOutgoingConnectionsAsync(CancellationToken cancellationToken);
    Task BlockConnectionAsync(NetworkConnection connection, CancellationToken cancellationToken);
}

public interface IPrivacyMonitorService
{
    event EventHandler<PrivacyEvent>? PrivacyEventDetected;
    Task StartAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<PrivacyEvent>> GetCurrentEventsAsync(CancellationToken cancellationToken);
}

public interface IStartupService
{
    Task<IReadOnlyList<StartupEntry>> GetStartupEntriesAsync(CancellationToken cancellationToken);
    Task DisableAsync(StartupEntry entry, CancellationToken cancellationToken);
}

public interface IFileActivityService
{
    event EventHandler<FileActivityEvent>? ActivityDetected;
    Task StartAsync(CancellationToken cancellationToken);
}

public interface IUsbMonitorService
{
    event EventHandler<UsbDeviceEvent>? DeviceChanged;
    Task StartAsync(CancellationToken cancellationToken);
}

public interface IFirewallService
{
    Task<string> GetStatusAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<FirewallRule>> GetRulesAsync(CancellationToken cancellationToken);
    Task CreateBlockRuleAsync(string name, string programPath, CancellationToken cancellationToken);
    Task CreateRemoteIpBlockRuleAsync(string name, string remoteIp, CancellationToken cancellationToken);
    Task RemoveRuleAsync(FirewallRule rule, CancellationToken cancellationToken);
}

public interface ISettingsService
{
    AppSettings Current { get; }
    Task LoadAsync(CancellationToken cancellationToken);
    Task SaveAsync(CancellationToken cancellationToken);
    Task SetAutoStartAsync(bool enabled, CancellationToken cancellationToken);
}

public interface INotificationService
{
    void Show(string title, string message, Severity severity);
}

public interface ITrayService : IDisposable
{
    void Initialize(System.Windows.Window mainWindow);
    void HideMainWindow();
    void ExitApplication();
}

public interface IUpdateService
{
    Task CheckForUpdatesAsync(CancellationToken cancellationToken);
}

public interface IThemeService
{
    void Apply(bool isDarkMode);
}

public interface ILocalizationService
{
    event EventHandler? LanguageChanged;
    string CurrentLanguage { get; }
    IReadOnlyList<string> SupportedLanguages { get; }
    string Get(string key);
    void SetLanguage(string language);
}

public interface IBackgroundMonitorService
{
    event EventHandler<IReadOnlyList<ProcessInfo>>? ProcessesUpdated;
    event EventHandler<IReadOnlyList<NetworkConnection>>? ConnectionsUpdated;
    event EventHandler<IReadOnlyList<StartupEntry>>? StartupEntriesUpdated;
    event EventHandler<(string Status, IReadOnlyList<FirewallRule> Rules)>? FirewallUpdated;

    IReadOnlyList<ProcessInfo> LatestProcesses { get; }
    IReadOnlyList<NetworkConnection> LatestConnections { get; }
    IReadOnlyList<StartupEntry> LatestStartupEntries { get; }
    IReadOnlyList<FirewallRule> LatestFirewallRules { get; }
    string FirewallStatus { get; }

    Task RefreshProcessesAsync(CancellationToken cancellationToken);
    Task RefreshConnectionsAsync(CancellationToken cancellationToken);
    Task RefreshStartupEntriesAsync(CancellationToken cancellationToken);
    Task RefreshFirewallAsync(CancellationToken cancellationToken);
}

public interface ILogRepository
{
    Task AddAsync(LogEntry entry, CancellationToken cancellationToken);
    Task<IReadOnlyList<LogEntry>> QueryAsync(DateTimeOffset? from, DateTimeOffset? to, Severity? severity, string? category, CancellationToken cancellationToken);
}

public interface IDatabaseInitializer
{
    Task InitializeAsync();
}

public interface IUiDispatcher
{
    void Invoke(Action action);
}
