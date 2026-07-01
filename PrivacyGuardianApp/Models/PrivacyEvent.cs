using PrivacyGuardian.Core;

namespace PrivacyGuardian.Models;

public sealed class PrivacyEvent
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
    public string Application { get; init; } = string.Empty;
    public string Resource { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public Severity Severity { get; init; } = Severity.Warning;
}
