using PrivacyGuardian.Core;

namespace PrivacyGuardian.Models;

public sealed class LogEntry
{
    public long Id { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
    public Severity Severity { get; init; } = Severity.Information;
    public string Category { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Details { get; init; } = string.Empty;
}
