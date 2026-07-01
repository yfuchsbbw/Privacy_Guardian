namespace PrivacyGuardian.Models;

public sealed class AppSettings
{
    public bool IsDarkMode { get; set; } = true;
    public bool NotificationsEnabled { get; set; } = true;
    public bool AutoStartEnabled { get; set; }
    public bool RunInBackgroundOnClose { get; set; } = true;
    public bool AutoUpdateEnabled { get; set; } = true;
    public bool AutoInstallUpdates { get; set; }
    public string UpdateManifestUrl { get; set; } = "https://raw.githubusercontent.com/yfuchsbbw/Privacy_Guardian/main/update-manifest.json";
    public string Language { get; set; } = "en-US";
}
