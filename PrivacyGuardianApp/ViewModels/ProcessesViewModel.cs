using System.Collections.ObjectModel;
using System.Windows.Input;
using PrivacyGuardian.Helpers;
using PrivacyGuardian.Models;
using PrivacyGuardian.Services;

namespace PrivacyGuardian.ViewModels;

public sealed class ProcessesViewModel : ObservableViewModel
{
    private readonly IBackgroundMonitorService _monitor;
    private readonly IUiDispatcher _dispatcher;
    private readonly ILocalizationService _localization;
    private string _searchText = string.Empty;

    public ProcessesViewModel(IBackgroundMonitorService monitor, IUiDispatcher dispatcher, ILocalizationService localization)
    {
        _monitor = monitor;
        _dispatcher = dispatcher;
        _localization = localization;
        _localization.LanguageChanged += (_, _) => RaiseLanguageProperties();
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        _monitor.ProcessesUpdated += (_, processes) => _dispatcher.Invoke(() => ReplaceProcesses(processes));
        ReplaceProcesses(_monitor.LatestProcesses);
        Status = "Monitoring running processes in the background";
        RaiseLanguageProperties();
    }

    public ObservableCollection<ProcessInfo> Processes { get; } = [];
    public ObservableCollection<ProcessInfo> FilteredProcesses { get; } = [];
    public ICommand RefreshCommand { get; }
    public string RefreshText => _localization.Get("Refresh");
    public string SearchTextLabel => _localization.Get("Search");

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ApplyFilter();
            }
        }
    }

    private async Task RefreshAsync()
    {
        IsBusy = true;
        Status = "Refreshing processes...";
        await _monitor.RefreshProcessesAsync(CancellationToken.None);
        IsBusy = false;
    }

    private void ReplaceProcesses(IReadOnlyList<ProcessInfo> processes)
    {
        Processes.Clear();
        foreach (var process in processes)
        {
            Processes.Add(process);
        }

        ApplyFilter();
        Status = $"{Processes.Count} processes monitored";
    }

    private void ApplyFilter()
    {
        FilteredProcesses.Clear();
        foreach (var process in Processes.Where(MatchesSearch))
        {
            FilteredProcesses.Add(process);
        }
    }

    private bool MatchesSearch(ProcessInfo process) =>
        string.IsNullOrWhiteSpace(SearchText) ||
        process.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
        process.ProcessId.ToString().Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
        process.Company.Contains(SearchText, StringComparison.OrdinalIgnoreCase);

    private void RaiseLanguageProperties()
    {
        Title = _localization.Get("Processes");
        OnPropertyChanged(nameof(RefreshText));
        OnPropertyChanged(nameof(SearchTextLabel));
    }
}
