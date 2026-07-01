using System.Collections.ObjectModel;
using System.Windows.Input;
using PrivacyGuardian.Core;
using PrivacyGuardian.Helpers;
using PrivacyGuardian.Models;
using PrivacyGuardian.Services;

namespace PrivacyGuardian.ViewModels;

public sealed class LogsViewModel : ObservableViewModel
{
    private readonly ILogRepository _logs;
    private readonly ILocalizationService _localization;
    private DateTime? _fromDate;
    private DateTime? _toDate;
    private string _selectedSeverity = AppText.All;
    private string _category = string.Empty;

    public LogsViewModel(ILogRepository logs, ILocalizationService localization)
    {
        _logs = logs;
        _localization = localization;
        _localization.LanguageChanged += (_, _) => RaiseLanguageProperties();
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        Status = "Click Refresh to load logs";
        RaiseLanguageProperties();
    }

    public ObservableCollection<LogEntry> Entries { get; } = [];
    public IReadOnlyList<string> Severities { get; } = [AppText.All, nameof(Severity.Information), nameof(Severity.Warning), nameof(Severity.Critical)];
    public ICommand RefreshCommand { get; }
    public string RefreshText => _localization.Get("Refresh");
    public string FromDateText => _localization.Get("FromDate");
    public string ToDateText => _localization.Get("ToDate");
    public string SeverityText => _localization.Get("Severity");
    public string CategoryText => _localization.Get("Category");
    public DateTime? FromDate { get => _fromDate; set => SetProperty(ref _fromDate, value); }
    public DateTime? ToDate { get => _toDate; set => SetProperty(ref _toDate, value); }
    public string SelectedSeverity { get => _selectedSeverity; set => SetProperty(ref _selectedSeverity, value); }
    public string Category { get => _category; set => SetProperty(ref _category, value); }

    private async Task RefreshAsync()
    {
        var severity = Enum.TryParse<Severity>(SelectedSeverity, out var parsed) ? parsed : (Severity?)null;
        DateTimeOffset? from = FromDate is null ? null : new DateTimeOffset(FromDate.Value);
        DateTimeOffset? to = ToDate is null ? null : new DateTimeOffset(ToDate.Value.AddDays(1));
        var entries = await _logs.QueryAsync(from, to, severity, string.IsNullOrWhiteSpace(Category) ? null : Category, CancellationToken.None);
        Entries.Clear();
        foreach (var entry in entries)
        {
            Entries.Add(entry);
        }
    }

    private void RaiseLanguageProperties()
    {
        Title = _localization.Get("Logs");
        OnPropertyChanged(nameof(RefreshText));
        OnPropertyChanged(nameof(FromDateText));
        OnPropertyChanged(nameof(ToDateText));
        OnPropertyChanged(nameof(SeverityText));
        OnPropertyChanged(nameof(CategoryText));
    }
}
