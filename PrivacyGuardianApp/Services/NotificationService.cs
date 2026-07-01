using PrivacyGuardian.Core;

namespace PrivacyGuardian.Services;

public sealed class NotificationService(ISettingsService settingsService) : INotificationService
{
    public void Show(string title, string message, Severity severity)
    {
        if (!settingsService.Current.NotificationsEnabled)
        {
            return;
        }

        // Keep monitoring non-blocking. A modal MessageBox would freeze the dashboard
        // whenever clipboard, camera, microphone, or USB events arrive.
    }
}
