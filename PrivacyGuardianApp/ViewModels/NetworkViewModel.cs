using System.Collections.ObjectModel;
using System.Windows.Input;
using PrivacyGuardian.Helpers;
using PrivacyGuardian.Models;
using PrivacyGuardian.Services;

namespace PrivacyGuardian.ViewModels;

public sealed class NetworkViewModel : ObservableViewModel
{
    private readonly INetworkService _networkService;
    private readonly IBackgroundMonitorService _monitor;
    private readonly IUiDispatcher _dispatcher;
    private readonly ILocalizationService _localization;

    public NetworkViewModel(INetworkService networkService, IBackgroundMonitorService monitor, IUiDispatcher dispatcher, ILocalizationService localization)
    {
        _networkService = networkService;
        _monitor = monitor;
        _dispatcher = dispatcher;
        _localization = localization;
        _localization.LanguageChanged += (_, _) => RaiseLanguageProperties();
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        BlockCommand = new AsyncRelayCommand(async parameter =>
        {
            if (parameter is NetworkConnection connection)
            {
                await _networkService.BlockConnectionAsync(connection, CancellationToken.None);
            }
        });
        _monitor.ConnectionsUpdated += (_, connections) => _dispatcher.Invoke(() => ReplaceConnections(connections));
        ReplaceConnections(_monitor.LatestConnections);
        Status = "Monitoring outgoing connections in the background";
        RaiseLanguageProperties();
    }

    public ObservableCollection<NetworkConnection> Connections { get; } = [];
    public ICommand RefreshCommand { get; }
    public ICommand BlockCommand { get; }
    public string RefreshText => _localization.Get("Refresh");
    public string BlockText => _localization.Get("Block");

    private async Task RefreshAsync()
    {
        IsBusy = true;
        Status = "Refreshing connections...";
        await _monitor.RefreshConnectionsAsync(CancellationToken.None);
        IsBusy = false;
    }

    private void ReplaceConnections(IReadOnlyList<NetworkConnection> connections)
    {
        Connections.Clear();
        foreach (var connection in connections)
        {
            Connections.Add(connection);
        }

        Status = $"{Connections.Count} outgoing connections monitored";
    }

    private void RaiseLanguageProperties()
    {
        Title = _localization.Get("Network");
        OnPropertyChanged(nameof(RefreshText));
        OnPropertyChanged(nameof(BlockText));
    }
}
