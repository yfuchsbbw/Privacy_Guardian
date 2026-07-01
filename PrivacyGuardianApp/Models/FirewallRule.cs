namespace PrivacyGuardian.Models;

public sealed class FirewallRule
{
    public string Name { get; init; } = string.Empty;
    public string Direction { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string Program { get; init; } = string.Empty;
    public bool Enabled { get; init; }
}
