using System.Collections.ObjectModel;
using System.Windows.Input;
using PrivacyGuardian.Helpers;
using PrivacyGuardian.Models;
using PrivacyGuardian.Services;

namespace PrivacyGuardian.ViewModels;

public sealed class FirewallViewModel : ObservableViewModel
{
    private readonly IFirewallService _firewallService;
    private readonly IBackgroundMonitorService _monitor;
    private readonly IUiDispatcher _dispatcher;
    private readonly ILocalizationService _localization;
    private string _statusText = string.Empty;
    private string _newRuleName = string.Empty;
    private string _newProgramPath = string.Empty;

    public FirewallViewModel(IFirewallService firewallService, IBackgroundMonitorService monitor, IUiDispatcher dispatcher, ILocalizationService localization)
    {
        _firewallService = firewallService;
        _monitor = monitor;
        _dispatcher = dispatcher;
        _localization = localization;
        _localization.LanguageChanged += (_, _) => RaiseLanguageProperties();
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        CreateRuleCommand = new AsyncRelayCommand(CreateRuleAsync, () => !string.IsNullOrWhiteSpace(NewRuleName) && !string.IsNullOrWhiteSpace(NewProgramPath));
        RemoveRuleCommand = new AsyncRelayCommand(async parameter =>
        {
            if (parameter is FirewallRule rule)
            {
                await _firewallService.RemoveRuleAsync(rule, CancellationToken.None);
                await RefreshAsync();
            }
        });
        _monitor.FirewallUpdated += (_, state) => _dispatcher.Invoke(() => ReplaceRules(state.Status, state.Rules));
        ReplaceRules(_monitor.FirewallStatus, _monitor.LatestFirewallRules);
        Status = "Monitoring firewall state in the background";
        RaiseLanguageProperties();
    }

    public ObservableCollection<FirewallRule> Rules { get; } = [];
    public ICommand RefreshCommand { get; }
    public ICommand CreateRuleCommand { get; }
    public ICommand RemoveRuleCommand { get; }
    public string RefreshText => _localization.Get("Refresh");
    public string CreateBlockRuleText => _localization.Get("CreateBlockRule");
    public string RemoveText => _localization.Get("Remove");
    public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }
    public string NewRuleName { get => _newRuleName; set => SetProperty(ref _newRuleName, value); }
    public string NewProgramPath { get => _newProgramPath; set => SetProperty(ref _newProgramPath, value); }

    private async Task RefreshAsync()
    {
        IsBusy = true;
        Status = "Refreshing firewall state...";
        await _monitor.RefreshFirewallAsync(CancellationToken.None);
        IsBusy = false;
    }

    private async Task CreateRuleAsync()
    {
        await _firewallService.CreateBlockRuleAsync(NewRuleName, NewProgramPath, CancellationToken.None);
        NewRuleName = string.Empty;
        NewProgramPath = string.Empty;
        await RefreshAsync();
    }

    private void ReplaceRules(string firewallStatus, IReadOnlyList<FirewallRule> rules)
    {
        StatusText = string.IsNullOrWhiteSpace(firewallStatus) ? "Loading..." : firewallStatus;
        Rules.Clear();
        foreach (var rule in rules)
        {
            Rules.Add(rule);
        }

        Status = $"{Rules.Count} firewall rules monitored";
    }

    private void RaiseLanguageProperties()
    {
        Title = _localization.Get("Firewall");
        OnPropertyChanged(nameof(RefreshText));
        OnPropertyChanged(nameof(CreateBlockRuleText));
        OnPropertyChanged(nameof(RemoveText));
    }
}
