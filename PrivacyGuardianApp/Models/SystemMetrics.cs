namespace PrivacyGuardian.Models;

public sealed record SystemMetrics(
    double CpuUsage,
    double RamUsage,
    double DiskUsage,
    double NetworkUploadBytes,
    double NetworkDownloadBytes,
    DateTimeOffset CapturedAt);
