using Microsoft.Extensions.Hosting;
using PrivacyGuardian.Models;

namespace PrivacyGuardian.Services;

public sealed class BackgroundMonitorService(
    IProcessService processService,
    INetworkService networkService,
    IStartupService startupService,
    IFirewallService firewallService) : BackgroundService, IBackgroundMonitorService
{
    private readonly SemaphoreSlim _processLock = new(1, 1);
    private readonly SemaphoreSlim _networkLock = new(1, 1);
    private readonly SemaphoreSlim _startupLock = new(1, 1);
    private readonly SemaphoreSlim _firewallLock = new(1, 1);

    public event EventHandler<IReadOnlyList<ProcessInfo>>? ProcessesUpdated;
    public event EventHandler<IReadOnlyList<NetworkConnection>>? ConnectionsUpdated;
    public event EventHandler<IReadOnlyList<StartupEntry>>? StartupEntriesUpdated;
    public event EventHandler<(string Status, IReadOnlyList<FirewallRule> Rules)>? FirewallUpdated;

    public IReadOnlyList<ProcessInfo> LatestProcesses { get; private set; } = [];
    public IReadOnlyList<NetworkConnection> LatestConnections { get; private set; } = [];
    public IReadOnlyList<StartupEntry> LatestStartupEntries { get; private set; } = [];
    public IReadOnlyList<FirewallRule> LatestFirewallRules { get; private set; } = [];
    public string FirewallStatus { get; private set; } = string.Empty;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = new[]
        {
            RunLoopAsync(RefreshProcessesAsync, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(20), stoppingToken),
            RunLoopAsync(RefreshConnectionsAsync, TimeSpan.FromSeconds(6), TimeSpan.FromSeconds(20), stoppingToken),
            RunLoopAsync(RefreshStartupEntriesAsync, TimeSpan.FromSeconds(20), TimeSpan.FromMinutes(5), stoppingToken),
            RunLoopAsync(RefreshFirewallAsync, TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(5), stoppingToken)
        };

        await Task.WhenAll(tasks);
    }

    public async Task RefreshProcessesAsync(CancellationToken cancellationToken)
    {
        if (!await _processLock.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            LatestProcesses = await processService.GetProcessesAsync(cancellationToken).ConfigureAwait(false);
            ProcessesUpdated?.Invoke(this, LatestProcesses);
        }
        catch
        {
            // Keep background monitoring alive even when one Windows API call fails.
        }
        finally
        {
            _processLock.Release();
        }
    }

    public async Task RefreshConnectionsAsync(CancellationToken cancellationToken)
    {
        if (!await _networkLock.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            LatestConnections = await networkService.GetOutgoingConnectionsAsync(cancellationToken).ConfigureAwait(false);
            ConnectionsUpdated?.Invoke(this, LatestConnections);
        }
        catch
        {
        }
        finally
        {
            _networkLock.Release();
        }
    }

    public async Task RefreshStartupEntriesAsync(CancellationToken cancellationToken)
    {
        if (!await _startupLock.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            LatestStartupEntries = await startupService.GetStartupEntriesAsync(cancellationToken).ConfigureAwait(false);
            StartupEntriesUpdated?.Invoke(this, LatestStartupEntries);
        }
        catch
        {
        }
        finally
        {
            _startupLock.Release();
        }
    }

    public async Task RefreshFirewallAsync(CancellationToken cancellationToken)
    {
        if (!await _firewallLock.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            FirewallStatus = await firewallService.GetStatusAsync(cancellationToken).ConfigureAwait(false);
            LatestFirewallRules = await firewallService.GetRulesAsync(cancellationToken).ConfigureAwait(false);
            FirewallUpdated?.Invoke(this, (FirewallStatus, LatestFirewallRules));
        }
        catch
        {
        }
        finally
        {
            _firewallLock.Release();
        }
    }

    private static async Task RunLoopAsync(Func<CancellationToken, Task> refresh, TimeSpan initialDelay, TimeSpan interval, CancellationToken cancellationToken)
    {
        if (initialDelay > TimeSpan.Zero)
        {
            await Task.Delay(initialDelay, cancellationToken).ConfigureAwait(false);
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            await refresh(cancellationToken).ConfigureAwait(false);
            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        }
    }
}
