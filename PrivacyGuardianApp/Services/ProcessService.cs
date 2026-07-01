using System.Diagnostics;
using System.IO;
using PrivacyGuardian.Models;

namespace PrivacyGuardian.Services;

public sealed class ProcessService : IProcessService
{
    private readonly Dictionary<int, (TimeSpan Cpu, DateTimeOffset Time)> _samples = new();

    public Task<IReadOnlyList<ProcessInfo>> GetProcessesAsync(CancellationToken cancellationToken)
    {
        return Task.Run<IReadOnlyList<ProcessInfo>>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var processorCount = Math.Max(1, Environment.ProcessorCount);
            var now = DateTimeOffset.Now;
            var processes = new List<ProcessInfo>();

            foreach (var process in Process.GetProcesses())
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var totalCpu = TryGet(() => process.TotalProcessorTime);
                    var cpuUsage = 0d;
                    if (_samples.TryGetValue(process.Id, out var previous))
                    {
                        var elapsed = Math.Max(0.1, (now - previous.Time).TotalSeconds);
                        cpuUsage = (totalCpu - previous.Cpu).TotalSeconds / elapsed / processorCount * 100;
                    }

                    _samples[process.Id] = (totalCpu, now);
                    var isSuspicious = IsSuspicious(process, out var reason);

                    processes.Add(new ProcessInfo
                    {
                        Name = process.ProcessName,
                        ProcessId = process.Id,
                        MemoryBytes = TryGet(() => process.WorkingSet64),
                        CpuUsage = Math.Clamp(cpuUsage, 0, 100),
                        DigitalSignature = "Optimized scan",
                        Company = "Available in detailed scan",
                        StartTime = TryGet<DateTime?>(() => process.StartTime),
                        FilePath = string.Empty,
                        IsSuspicious = isSuspicious,
                        SuspicionReason = reason
                    });
                }
                catch
                {
                    // Some protected system processes reject inspection; skip details safely.
                }
            }

            var activeIds = processes.Select(p => p.ProcessId).ToHashSet();
            foreach (var stale in _samples.Keys.Where(id => !activeIds.Contains(id)).ToArray())
            {
                _samples.Remove(stale);
            }

            return processes.OrderByDescending(p => p.IsSuspicious).ThenBy(p => p.Name).ToList();
        }, cancellationToken);
    }

    private static bool IsSuspicious(Process process, out string reason)
    {
        if (process.ProcessName.Length <= 2)
        {
            reason = "Unusually short process name";
            return true;
        }

        reason = string.Empty;
        return false;
    }

    private static T TryGet<T>(Func<T> getter)
    {
        try
        {
            return getter();
        }
        catch
        {
            return default!;
        }
    }
}
