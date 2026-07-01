namespace PrivacyGuardian.Models;

public sealed class NetworkConnection
{
    public string Application { get; init; } = string.Empty;
    public string Protocol { get; init; } = string.Empty;
    public string LocalIp { get; init; } = string.Empty;
    public string RemoteIp { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string Hostname { get; init; } = string.Empty;
    public int Port { get; init; }
    public double UploadBytesPerSecond { get; init; }
    public double DownloadBytesPerSecond { get; init; }
    public int ProcessId { get; init; }
    public string ExecutablePath { get; init; } = string.Empty;
}
