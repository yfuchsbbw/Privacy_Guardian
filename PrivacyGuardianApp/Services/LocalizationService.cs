namespace PrivacyGuardian.Services;

public sealed class LocalizationService : ILocalizationService
{
    private readonly Dictionary<string, Dictionary<string, string>> _languages = new(StringComparer.OrdinalIgnoreCase)
    {
        ["en-US"] = new()
        {
            ["Dashboard"] = "Dashboard",
            ["Processes"] = "Running Processes",
            ["Network"] = "Network Monitor",
            ["Privacy"] = "Privacy Monitor",
            ["Startup"] = "Startup Manager",
            ["FileActivity"] = "File Activity",
            ["Usb"] = "USB Monitor",
            ["Firewall"] = "Firewall",
            ["Logs"] = "Logs",
            ["Settings"] = "Settings",
            ["Refresh"] = "Refresh",
            ["Block"] = "Block",
            ["Remove"] = "Remove",
            ["Disable"] = "Disable",
            ["CreateBlockRule"] = "Create Block Rule",
            ["Search"] = "Search",
            ["SaveSettings"] = "Save Settings",
            ["DarkMode"] = "Dark Mode",
            ["Notifications"] = "Notifications",
            ["AutoStart"] = "Auto Start",
            ["RunInBackground"] = "Keep running after closing",
            ["AutoUpdate"] = "Check for updates",
            ["AutoInstallUpdates"] = "Install updates automatically",
            ["UpdateManifestUrl"] = "Update manifest URL",
            ["Language"] = "Language",
            ["MonitoringActive"] = "Background monitoring active",
            ["StatusReady"] = "Ready",
            ["CpuUsage"] = "CPU Usage",
            ["RamUsage"] = "RAM Usage",
            ["DiskUsage"] = "Disk Usage",
            ["NetworkUpload"] = "Network Upload",
            ["NetworkDownload"] = "Network Download",
            ["FromDate"] = "From",
            ["ToDate"] = "To",
            ["Severity"] = "Severity",
            ["Category"] = "Category"
        },
        ["de-CH"] = new()
        {
            ["Dashboard"] = "Dashboard",
            ["Processes"] = "Laufende Prozesse",
            ["Network"] = "Netzwerkmonitor",
            ["Privacy"] = "Privatsph\u00e4re-Monitor",
            ["Startup"] = "Autostart-Manager",
            ["FileActivity"] = "Dateiaktivit\u00e4t",
            ["Usb"] = "USB-Monitor",
            ["Firewall"] = "Firewall",
            ["Logs"] = "Protokolle",
            ["Settings"] = "Einstellungen",
            ["Refresh"] = "Aktualisieren",
            ["Block"] = "Blockieren",
            ["Remove"] = "Entfernen",
            ["Disable"] = "Deaktivieren",
            ["CreateBlockRule"] = "Block-Regel erstellen",
            ["Search"] = "Suchen",
            ["SaveSettings"] = "Einstellungen speichern",
            ["DarkMode"] = "Dunkler Modus",
            ["Notifications"] = "Benachrichtigungen",
            ["AutoStart"] = "Automatisch starten",
            ["RunInBackground"] = "Nach dem Schliessen weiterlaufen",
            ["AutoUpdate"] = "Nach Updates suchen",
            ["AutoInstallUpdates"] = "Updates automatisch installieren",
            ["UpdateManifestUrl"] = "Update-Manifest-URL",
            ["Language"] = "Sprache",
            ["MonitoringActive"] = "Hintergrund\u00fcberwachung aktiv",
            ["StatusReady"] = "Bereit",
            ["CpuUsage"] = "CPU-Auslastung",
            ["RamUsage"] = "RAM-Auslastung",
            ["DiskUsage"] = "Datentr\u00e4gerauslastung",
            ["NetworkUpload"] = "Netzwerk Upload",
            ["NetworkDownload"] = "Netzwerk Download",
            ["FromDate"] = "Von",
            ["ToDate"] = "Bis",
            ["Severity"] = "Schweregrad",
            ["Category"] = "Kategorie"
        },
        ["fr-FR"] = new()
        {
            ["Dashboard"] = "Tableau de bord",
            ["Processes"] = "Processus en cours",
            ["Network"] = "Moniteur r\u00e9seau",
            ["Privacy"] = "Moniteur de confidentialit\u00e9",
            ["Startup"] = "Gestion du d\u00e9marrage",
            ["FileActivity"] = "Activit\u00e9 fichiers",
            ["Usb"] = "Moniteur USB",
            ["Firewall"] = "Pare-feu",
            ["Logs"] = "Journaux",
            ["Settings"] = "Param\u00e8tres",
            ["Refresh"] = "Actualiser",
            ["Block"] = "Bloquer",
            ["Remove"] = "Supprimer",
            ["Disable"] = "D\u00e9sactiver",
            ["CreateBlockRule"] = "Cr\u00e9er une r\u00e8gle",
            ["Search"] = "Rechercher",
            ["SaveSettings"] = "Enregistrer",
            ["DarkMode"] = "Mode sombre",
            ["Notifications"] = "Notifications",
            ["AutoStart"] = "D\u00e9marrage auto",
            ["RunInBackground"] = "Continuer en arri\u00e8re-plan",
            ["AutoUpdate"] = "Rechercher les mises \u00e0 jour",
            ["AutoInstallUpdates"] = "Installer automatiquement",
            ["UpdateManifestUrl"] = "URL du manifeste",
            ["Language"] = "Langue",
            ["MonitoringActive"] = "Surveillance en arri\u00e8re-plan active",
            ["StatusReady"] = "Pr\u00eat",
            ["CpuUsage"] = "Utilisation CPU",
            ["RamUsage"] = "Utilisation RAM",
            ["DiskUsage"] = "Utilisation disque",
            ["NetworkUpload"] = "Envoi r\u00e9seau",
            ["NetworkDownload"] = "R\u00e9ception r\u00e9seau",
            ["FromDate"] = "Du",
            ["ToDate"] = "Au",
            ["Severity"] = "Gravit\u00e9",
            ["Category"] = "Cat\u00e9gorie"
        }
    };

    public event EventHandler? LanguageChanged;
    public string CurrentLanguage { get; private set; } = "en-US";
    public IReadOnlyList<string> SupportedLanguages { get; } = ["en-US", "de-CH", "fr-FR"];

    public string Get(string key)
    {
        if (_languages.TryGetValue(CurrentLanguage, out var language) && language.TryGetValue(key, out var value))
        {
            return value;
        }

        return _languages["en-US"].TryGetValue(key, out var fallback) ? fallback : key;
    }

    public void SetLanguage(string language)
    {
        if (!_languages.ContainsKey(language) || CurrentLanguage.Equals(language, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        CurrentLanguage = language;
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
