using PrivacyGuardian.Core;

namespace PrivacyGuardian.Models;

public sealed class FileActivityEvent
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
    public string Path { get; init; } = string.Empty;
    public string OldPath { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public Severity Severity { get; init; } = Severity.Information;
}
