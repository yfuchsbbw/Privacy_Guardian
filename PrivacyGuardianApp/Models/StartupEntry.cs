namespace PrivacyGuardian.Models;

public sealed class StartupEntry
{
    public string Name { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string Command { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public string Location { get; init; } = string.Empty;
}
