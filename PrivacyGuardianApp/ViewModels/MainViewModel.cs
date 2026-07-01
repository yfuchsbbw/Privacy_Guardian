using System.ComponentModel;
using System.Windows.Input;
using PrivacyGuardian.Core;
using PrivacyGuardian.Helpers;
using PrivacyGuardian.Services;

namespace PrivacyGuardian.ViewModels;

public sealed class MainViewModel : ObservableViewModel
{
    private readonly INavigationService _navigation;
    private readonly ILocalizationService _localization;
    private ObservableViewModel? _observedViewModel;

    public MainViewModel(INavigationService navigation, ISettingsService settings, IThemeService theme, ILocalizationService localization, IPrivacyMonitorService privacyMonitor, IFileActivityService fileActivity, IUsbMonitorService usbMonitor)
    {
        _navigation = navigation;
        _localization = localization;
        ObserveCurrentViewModel();
        _navigation.CurrentViewModelChanged += (_, _) =>
        {
            ObserveCurrentViewModel();
            OnPropertyChanged(nameof(CurrentViewModel));
            OnPropertyChanged(nameof(CurrentTitle));
            OnPropertyChanged(nameof(CurrentStatus));
        };
        _localization.LanguageChanged += (_, _) => RaiseLanguageProperties();
        _ = privacyMonitor.StartAsync(CancellationToken.None);
        _ = fileActivity.StartAsync(CancellationToken.None);
        _ = usbMonitor.StartAsync(CancellationToken.None);

        NavigateDashboardCommand = new RelayCommand(() => _navigation.NavigateTo<DashboardViewModel>());
        NavigateProcessesCommand = new RelayCommand(() => _navigation.NavigateTo<ProcessesViewModel>());
        NavigateNetworkCommand = new RelayCommand(() => _navigation.NavigateTo<NetworkViewModel>());
        NavigatePrivacyCommand = new RelayCommand(() => _navigation.NavigateTo<PrivacyViewModel>());
        NavigateStartupCommand = new RelayCommand(() => _navigation.NavigateTo<StartupViewModel>());
        NavigateFileActivityCommand = new RelayCommand(() => _navigation.NavigateTo<FileActivityViewModel>());
        NavigateUsbCommand = new RelayCommand(() => _navigation.NavigateTo<UsbViewModel>());
        NavigateFirewallCommand = new RelayCommand(() => _navigation.NavigateTo<FirewallViewModel>());
        NavigateLogsCommand = new RelayCommand(() => _navigation.NavigateTo<LogsViewModel>());
        NavigateSettingsCommand = new RelayCommand(() => _navigation.NavigateTo<SettingsViewModel>());
    }

    public string AppName => AppText.AppName;
    public ObservableViewModel CurrentViewModel => _navigation.CurrentViewModel;
    public string CurrentTitle => string.IsNullOrWhiteSpace(CurrentViewModel.Title) ? AppName : CurrentViewModel.Title;
    public string DashboardText => _localization.Get("Dashboard");
    public string ProcessesText => _localization.Get("Processes");
    public string NetworkText => _localization.Get("Network");
    public string PrivacyText => _localization.Get("Privacy");
    public string StartupText => _localization.Get("Startup");
    public string FileActivityText => _localization.Get("FileActivity");
    public string UsbText => _localization.Get("Usb");
    public string FirewallText => _localization.Get("Firewall");
    public string LogsText => _localization.Get("Logs");
    public string SettingsText => _localization.Get("Settings");
    public string MonitoringText => _localization.Get("MonitoringActive");
    public string CurrentStatus => string.IsNullOrWhiteSpace(CurrentViewModel.Status) ? _localization.Get("StatusReady") : CurrentViewModel.Status;
    public ICommand NavigateDashboardCommand { get; }
    public ICommand NavigateProcessesCommand { get; }
    public ICommand NavigateNetworkCommand { get; }
    public ICommand NavigatePrivacyCommand { get; }
    public ICommand NavigateStartupCommand { get; }
    public ICommand NavigateFileActivityCommand { get; }
    public ICommand NavigateUsbCommand { get; }
    public ICommand NavigateFirewallCommand { get; }
    public ICommand NavigateLogsCommand { get; }
    public ICommand NavigateSettingsCommand { get; }

    private void RaiseLanguageProperties()
    {
        OnPropertyChanged(nameof(DashboardText));
        OnPropertyChanged(nameof(ProcessesText));
        OnPropertyChanged(nameof(NetworkText));
        OnPropertyChanged(nameof(PrivacyText));
        OnPropertyChanged(nameof(StartupText));
        OnPropertyChanged(nameof(FileActivityText));
        OnPropertyChanged(nameof(UsbText));
        OnPropertyChanged(nameof(FirewallText));
        OnPropertyChanged(nameof(LogsText));
        OnPropertyChanged(nameof(SettingsText));
        OnPropertyChanged(nameof(MonitoringText));
        OnPropertyChanged(nameof(CurrentTitle));
        OnPropertyChanged(nameof(CurrentStatus));
    }

    private void ObserveCurrentViewModel()
    {
        if (_observedViewModel is not null)
        {
            _observedViewModel.PropertyChanged -= OnCurrentViewModelPropertyChanged;
        }

        _observedViewModel = _navigation.CurrentViewModel;
        _observedViewModel.PropertyChanged += OnCurrentViewModelPropertyChanged;
    }

    private void OnCurrentViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ObservableViewModel.Status) or nameof(ObservableViewModel.Title))
        {
            OnPropertyChanged(nameof(CurrentStatus));
            OnPropertyChanged(nameof(CurrentTitle));
        }
    }
}
