using PrivacyGuardian.Core;

namespace PrivacyGuardian.Models;

public sealed class UsbDeviceEvent
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
    public string Action { get; init; } = string.Empty;
    public string Vendor { get; init; } = string.Empty;
    public string Product { get; init; } = string.Empty;
    public string SerialNumber { get; init; } = string.Empty;
    public Severity Severity { get; init; } = Severity.Warning;
}
