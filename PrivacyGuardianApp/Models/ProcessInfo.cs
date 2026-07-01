namespace PrivacyGuardian.Models;

public sealed class ProcessInfo
{
    public string Name { get; init; } = string.Empty;
    public int ProcessId { get; init; }
    public long MemoryBytes { get; init; }
    public double CpuUsage { get; init; }
    public string DigitalSignature { get; init; } = string.Empty;
    public string Company { get; init; } = string.Empty;
    public DateTime? StartTime { get; init; }
    public string FilePath { get; init; } = string.Empty;
    public bool IsSuspicious { get; init; }
    public string SuspicionReason { get; init; } = string.Empty;
}
