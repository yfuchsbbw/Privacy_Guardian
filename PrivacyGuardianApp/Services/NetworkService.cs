using System.Diagnostics;
using System.Net;
using PrivacyGuardian.Models;

namespace PrivacyGuardian.Services;

public sealed class NetworkService(IFirewallService firewallService) : INetworkService
{
    public async Task<IReadOnlyList<NetworkConnection>> GetOutgoingConnectionsAsync(CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        var startInfo = new ProcessStartInfo("netstat", "-ano -p tcp")
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            return [];
        }

        var output = await process.StandardOutput.ReadToEndAsync(timeout.Token).ConfigureAwait(false);
        await process.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
        return await Task.Run<IReadOnlyList<NetworkConnection>>(() =>
            output.Split(Environment.NewLine)
                .Select(ParseTcpLine)
                .Where(c => c is not null)
                .Cast<NetworkConnection>()
                .ToList(), timeout.Token).ConfigureAwait(false);
    }

    public Task BlockConnectionAsync(NetworkConnection connection, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(connection.ExecutablePath))
        {
            return firewallService.CreateRemoteIpBlockRuleAsync(connection.RemoteIp, connection.RemoteIp, cancellationToken);
        }

        return firewallService.CreateBlockRuleAsync(connection.Application, connection.ExecutablePath, cancellationToken);
    }

    private static NetworkConnection? ParseTcpLine(string line)
    {
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 5 || !parts[0].Equals("TCP", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (IsIgnoredTcpState(parts[3]))
        {
            return null;
        }

        var pid = int.TryParse(parts[4], out var parsedPid) ? parsedPid : 0;
        var remote = SplitEndpoint(parts[2]);
        if (IPAddress.TryParse(remote.Ip, out var remoteIp) && IPAddress.IsLoopback(remoteIp))
        {
            return null;
        }

        var local = SplitEndpoint(parts[1]);
        var name = GetProcessName(pid);
        return new NetworkConnection
        {
            Application = name,
            Protocol = "TCP",
            LocalIp = local.Ip,
            RemoteIp = remote.Ip,
            Country = "Unknown",
            Hostname = string.Empty,
            Port = remote.Port,
            ProcessId = pid,
            ExecutablePath = string.Empty
        };
    }

    private static (string Ip, int Port) SplitEndpoint(string endpoint)
    {
        var index = endpoint.LastIndexOf(':');
        if (index <= 0)
        {
            return (endpoint, 0);
        }

        return (endpoint[..index].Trim('[', ']'), int.TryParse(endpoint[(index + 1)..], out var port) ? port : 0);
    }

    private static bool IsIgnoredTcpState(string state)
    {
        var normalized = state.Trim().ToUpperInvariant();
        return normalized is
            "LISTENING" or
            "ABH\u00d6REN" or
            "ABHOEREN" or
            "TIME_WAIT" or
            "CLOSE_WAIT" or
            "CLOSED" or
            "SCHLIESSEN" or
            "SYN_SENT" or
            "SYN_RECEIVED" or
            "FIN_WAIT_1" or
            "FIN_WAIT_2" or
            "LAST_ACK";
    }

    private static string GetProcessName(int pid)
    {
        try
        {
            using var process = Process.GetProcessById(pid);
            return process.ProcessName;
        }
        catch
        {
            return $"PID {pid}";
        }
    }

}
