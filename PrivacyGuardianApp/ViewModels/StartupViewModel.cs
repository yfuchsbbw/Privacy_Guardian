using System.Collections.ObjectModel;
using System.Windows.Input;
using PrivacyGuardian.Helpers;
using PrivacyGuardian.Models;
using PrivacyGuardian.Services;

namespace PrivacyGuardian.ViewModels;

public sealed class StartupViewModel : ObservableViewModel
{
    private readonly IStartupService _startupService;
    private readonly IBackgroundMonitorService _monitor;
    private readonly IUiDispatcher _dispatcher;
    private readonly ILocalizationService _localization;

    public StartupViewModel(IStartupService startupService, IBackgroundMonitorService monitor, IUiDispatcher dispatcher, ILocalizationService localization)
    {
        _startupService = startupService;
        _monitor = monitor;
        _dispatcher = dispatcher;
        _localization = localization;
        _localization.LanguageChanged += (_, _) => RaiseLanguageProperties();
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        DisableCommand = new AsyncRelayCommand(async parameter =>
        {
            if (parameter is StartupEntry entry)
            {
                await _startupService.DisableAsync(entry, CancellationToken.None);
                await RefreshAsync();
            }
        });
        _monitor.StartupEntriesUpdated += (_, entries) => _dispatcher.Invoke(() => ReplaceEntries(entries));
        ReplaceEntries(_monitor.LatestStartupEntries);
        Status = "Monitoring startup entries in the background";
        RaiseLanguageProperties();
    }

    public ObservableCollection<StartupEntry> Entries { get; } = [];
    public ICommand RefreshCommand { get; }
    public ICommand DisableCommand { get; }
    public string RefreshText => _localization.Get("Refresh");
    public string DisableText => _localization.Get("Disable");

    private async Task RefreshAsync()
    {
        IsBusy = true;
        Status = "Refreshing startup entries...";
        await _monitor.RefreshStartupEntriesAsync(CancellationToken.None);
        IsBusy = false;
    }

    private void ReplaceEntries(IReadOnlyList<StartupEntry> entries)
    {
        Entries.Clear();
        foreach (var entry in entries)
        {
            Entries.Add(entry);
        }

        Status = $"{Entries.Count} startup entries monitored";
    }

    private void RaiseLanguageProperties()
    {
        Title = _localization.Get("Startup");
        OnPropertyChanged(nameof(RefreshText));
        OnPropertyChanged(nameof(DisableText));
    }
}
