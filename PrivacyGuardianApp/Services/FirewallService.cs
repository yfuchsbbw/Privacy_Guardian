using System.Diagnostics;
using PrivacyGuardian.Core;
using PrivacyGuardian.Models;

namespace PrivacyGuardian.Services;

public sealed class FirewallService(ILogRepository logs) : IFirewallService
{
    public async Task<string> GetStatusAsync(CancellationToken cancellationToken)
    {
        var output = await RunNetshAsync("advfirewall show allprofiles state", cancellationToken);
        return output.Contains("ON", StringComparison.OrdinalIgnoreCase) ? "Enabled" : "Review required";
    }

    public async Task<IReadOnlyList<FirewallRule>> GetRulesAsync(CancellationToken cancellationToken)
    {
        var output = await RunNetshAsync("advfirewall firewall show rule name=all", cancellationToken);
        return output.Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries)
            .Where(block => block.Contains(Constants.FirewallRulePrefix, StringComparison.OrdinalIgnoreCase))
            .Select(ParseRule)
            .ToList();
    }

    public async Task CreateBlockRuleAsync(string name, string programPath, CancellationToken cancellationToken)
    {
        var safeName = Constants.FirewallRulePrefix + name.Replace('"', '\'');
        var arguments = $"advfirewall firewall add rule name=\"{safeName}\" dir=out action=block program=\"{programPath}\" enable=yes";
        await RunNetshAsync(arguments, cancellationToken);
        await logs.AddAsync(new LogEntry { Category = "Firewall", Severity = Severity.Warning, Message = "Block rule created", Details = safeName }, cancellationToken);
    }

    public async Task CreateRemoteIpBlockRuleAsync(string name, string remoteIp, CancellationToken cancellationToken)
    {
        var safeName = Constants.FirewallRulePrefix + name.Replace('"', '\'');
        var arguments = $"advfirewall firewall add rule name=\"{safeName}\" dir=out action=block remoteip={remoteIp} enable=yes";
        await RunNetshAsync(arguments, cancellationToken);
        await logs.AddAsync(new LogEntry { Category = "Firewall", Severity = Severity.Warning, Message = "Remote IP block rule created", Details = $"{safeName} -> {remoteIp}" }, cancellationToken);
    }

    public async Task RemoveRuleAsync(FirewallRule rule, CancellationToken cancellationToken)
    {
        await RunNetshAsync($"advfirewall firewall delete rule name=\"{rule.Name}\"", cancellationToken);
        await logs.AddAsync(new LogEntry { Category = "Firewall", Severity = Severity.Information, Message = "Firewall rule removed", Details = rule.Name }, cancellationToken);
    }

    private static FirewallRule ParseRule(string block)
    {
        string Field(string label)
        {
            var line = block.Split('\n').FirstOrDefault(l => l.TrimStart().StartsWith(label, StringComparison.OrdinalIgnoreCase));
            return line is null ? string.Empty : line[(line.IndexOf(':') + 1)..].Trim();
        }

        return new FirewallRule
        {
            Name = Field("Rule Name"),
            Enabled = Field("Enabled").Equals("Yes", StringComparison.OrdinalIgnoreCase),
            Direction = Field("Direction"),
            Action = Field("Action"),
            Program = Field("Program")
        };
    }

    private static async Task<string> RunNetshAsync(string arguments, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(8));
        var startInfo = new ProcessStartInfo("netsh", arguments)
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Unable to start netsh.");
        var output = await process.StandardOutput.ReadToEndAsync(timeout.Token);
        var error = await process.StandardError.ReadToEndAsync(timeout.Token);
        await process.WaitForExitAsync(timeout.Token);
        return string.IsNullOrWhiteSpace(output) ? error : output;
    }
}
