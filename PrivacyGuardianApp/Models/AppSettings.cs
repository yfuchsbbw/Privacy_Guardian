namespace PrivacyGuardian.Models;

public sealed class AppSettings
{
    public bool IsDarkMode { get; set; } = true;
    public bool NotificationsEnabled { get; set; } = true;
    public bool AutoStartEnabled { get; set; }
    public bool RunInBackgroundOnClose { get; set; } = true;
    public bool AutoUpdateEnabled { get; set; } = true;
    public bool AutoInstallUpdates { get; set; }
    public string UpdateManifestUrl { get; set; } = string.Empty;
    public string Language { get; set; } = "en-US";
}
