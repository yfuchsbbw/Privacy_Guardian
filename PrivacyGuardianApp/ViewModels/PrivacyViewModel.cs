using System.Collections.ObjectModel;
using PrivacyGuardian.Models;
using PrivacyGuardian.Services;

namespace PrivacyGuardian.ViewModels;

public sealed class PrivacyViewModel : ObservableViewModel
{
    private readonly IUiDispatcher _dispatcher;

    public PrivacyViewModel(IPrivacyMonitorService privacyMonitor, IUiDispatcher dispatcher, ILocalizationService localization)
    {
        _dispatcher = dispatcher;
        localization.LanguageChanged += (_, _) => Title = localization.Get("Privacy");
        Title = localization.Get("Privacy");
        privacyMonitor.PrivacyEventDetected += (_, e) => _dispatcher.Invoke(() => Events.Insert(0, e));
        _ = LoadAsync(privacyMonitor);
    }

    public ObservableCollection<PrivacyEvent> Events { get; } = [];

    private async Task LoadAsync(IPrivacyMonitorService privacyMonitor)
    {
        var events = await privacyMonitor.GetCurrentEventsAsync(CancellationToken.None);
        _dispatcher.Invoke(() =>
        {
            Events.Clear();
            foreach (var privacyEvent in events)
            {
                Events.Add(privacyEvent);
            }
        });
    }
}
