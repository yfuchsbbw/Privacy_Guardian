using System.Windows.Input;
using PrivacyGuardian.Helpers;
using PrivacyGuardian.Services;

namespace PrivacyGuardian.ViewModels;

public sealed class SettingsViewModel : ObservableViewModel
{
    private readonly ISettingsService _settings;
    private readonly IThemeService _theme;
    private readonly ILocalizationService _localization;

    public SettingsViewModel(ISettingsService settings, IThemeService theme, ILocalizationService localization)
    {
        _settings = settings;
        _theme = theme;
        _localization = localization;
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        _localization.LanguageChanged += (_, _) => RaiseLanguageProperties();
        Title = _localization.Get("Settings");
    }

    public IReadOnlyList<string> Languages => _localization.SupportedLanguages;
    public string DarkModeText => _localization.Get("DarkMode");
    public string NotificationsText => _localization.Get("Notifications");
    public string AutoStartText => _localization.Get("AutoStart");
    public string RunInBackgroundText => _localization.Get("RunInBackground");
    public string AutoUpdateText => _localization.Get("AutoUpdate");
    public string AutoInstallUpdatesText => _localization.Get("AutoInstallUpdates");
    public string UpdateManifestUrlText => _localization.Get("UpdateManifestUrl");
    public string LanguageText => _localization.Get("Language");
    public string SaveSettingsText => _localization.Get("SaveSettings");
    public bool IsDarkMode { get => _settings.Current.IsDarkMode; set { _settings.Current.IsDarkMode = value; _theme.Apply(value); OnPropertyChanged(); } }
    public bool NotificationsEnabled { get => _settings.Current.NotificationsEnabled; set { _settings.Current.NotificationsEnabled = value; OnPropertyChanged(); } }
    public bool AutoStartEnabled { get => _settings.Current.AutoStartEnabled; set { _settings.Current.AutoStartEnabled = value; OnPropertyChanged(); } }
    public bool RunInBackgroundOnClose { get => _settings.Current.RunInBackgroundOnClose; set { _settings.Current.RunInBackgroundOnClose = value; OnPropertyChanged(); } }
    public bool AutoUpdateEnabled { get => _settings.Current.AutoUpdateEnabled; set { _settings.Current.AutoUpdateEnabled = value; OnPropertyChanged(); } }
    public bool AutoInstallUpdates { get => _settings.Current.AutoInstallUpdates; set { _settings.Current.AutoInstallUpdates = value; OnPropertyChanged(); } }
    public string UpdateManifestUrl { get => _settings.Current.UpdateManifestUrl; set { _settings.Current.UpdateManifestUrl = value; OnPropertyChanged(); } }
    public string Language { get => _settings.Current.Language; set { _settings.Current.Language = value; _localization.SetLanguage(value); OnPropertyChanged(); } }
    public ICommand SaveCommand { get; }

    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;
            await _settings.SetAutoStartAsync(AutoStartEnabled, CancellationToken.None);
            await _settings.SaveAsync(CancellationToken.None);
            Status = "Settings saved";
        }
        catch (Exception ex)
        {
            Status = $"Settings could not be saved: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void RaiseLanguageProperties()
    {
        Title = _localization.Get("Settings");
        OnPropertyChanged(nameof(DarkModeText));
        OnPropertyChanged(nameof(NotificationsText));
        OnPropertyChanged(nameof(AutoStartText));
        OnPropertyChanged(nameof(RunInBackgroundText));
        OnPropertyChanged(nameof(AutoUpdateText));
        OnPropertyChanged(nameof(AutoInstallUpdatesText));
        OnPropertyChanged(nameof(UpdateManifestUrlText));
        OnPropertyChanged(nameof(LanguageText));
        OnPropertyChanged(nameof(SaveSettingsText));
    }
}
