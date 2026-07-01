using System.Windows.Threading;
using PrivacyGuardian.Helpers;
using PrivacyGuardian.Services;

namespace PrivacyGuardian.ViewModels;

public sealed class DashboardViewModel : ObservableViewModel
{
    private readonly ISystemMetricsService _metricsService;
    private readonly ILocalizationService _localization;
    private readonly DispatcherTimer _timer;
    private double _cpuUsage;
    private double _ramUsage;
    private double _diskUsage;
    private string _networkUpload = "0 B/s";
    private string _networkDownload = "0 B/s";

    public DashboardViewModel(ISystemMetricsService metricsService, ILocalizationService localization)
    {
        _metricsService = metricsService;
        _localization = localization;
        _localization.LanguageChanged += (_, _) => RaiseLanguageProperties();
        RaiseLanguageProperties();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += async (_, _) => await RefreshAsync();
        _timer.Start();
        _ = RefreshAsync();
    }

    public double CpuUsage { get => _cpuUsage; set => SetProperty(ref _cpuUsage, value); }
    public double RamUsage { get => _ramUsage; set => SetProperty(ref _ramUsage, value); }
    public double DiskUsage { get => _diskUsage; set => SetProperty(ref _diskUsage, value); }
    public string CpuText => FormatHelper.Percent(CpuUsage);
    public string RamText => FormatHelper.Percent(RamUsage);
    public string DiskText => FormatHelper.Percent(DiskUsage);
    public string CpuUsageText => _localization.Get("CpuUsage");
    public string RamUsageText => _localization.Get("RamUsage");
    public string DiskUsageText => _localization.Get("DiskUsage");
    public string NetworkUploadText => _localization.Get("NetworkUpload");
    public string NetworkDownloadText => _localization.Get("NetworkDownload");
    public string NetworkUpload { get => _networkUpload; set => SetProperty(ref _networkUpload, value); }
    public string NetworkDownload { get => _networkDownload; set => SetProperty(ref _networkDownload, value); }

    private async Task RefreshAsync()
    {
        var metrics = await _metricsService.GetMetricsAsync(CancellationToken.None);
        CpuUsage = metrics.CpuUsage;
        RamUsage = metrics.RamUsage;
        DiskUsage = metrics.DiskUsage;
        NetworkUpload = FormatHelper.BytesPerSecond(metrics.NetworkUploadBytes);
        NetworkDownload = FormatHelper.BytesPerSecond(metrics.NetworkDownloadBytes);
        OnPropertyChanged(nameof(CpuText));
        OnPropertyChanged(nameof(RamText));
        OnPropertyChanged(nameof(DiskText));
    }

    private void RaiseLanguageProperties()
    {
        Title = _localization.Get("Dashboard");
        OnPropertyChanged(nameof(CpuUsageText));
        OnPropertyChanged(nameof(RamUsageText));
        OnPropertyChanged(nameof(DiskUsageText));
        OnPropertyChanged(nameof(NetworkUploadText));
        OnPropertyChanged(nameof(NetworkDownloadText));
    }
}
