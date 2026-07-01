using System.IO;
using System.Text.Json;
using Microsoft.Win32;
using PrivacyGuardian.Core;
using PrivacyGuardian.Models;

namespace PrivacyGuardian.Services;

public sealed class SettingsService : ISettingsService
{
    private readonly string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        Constants.ProductName,
        "settings.json");

    public AppSettings Current { get; private set; } = new();

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_settingsPath))
        {
            return;
        }

        await using var stream = File.OpenRead(_settingsPath);
        Current = await JsonSerializer.DeserializeAsync<AppSettings>(stream, cancellationToken: cancellationToken) ?? new AppSettings();
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
        await using var stream = File.Create(_settingsPath);
        await JsonSerializer.SerializeAsync(stream, Current, new JsonSerializerOptions { WriteIndented = true }, cancellationToken);
    }

    public async Task SetAutoStartAsync(bool enabled, CancellationToken cancellationToken)
    {
        Current.AutoStartEnabled = enabled;
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
        if (key is not null)
        {
            if (enabled)
            {
                key.SetValue(Constants.ProductName, Environment.ProcessPath ?? string.Empty);
            }
            else
            {
                key.DeleteValue(Constants.ProductName, false);
            }
        }

        await SaveAsync(cancellationToken);
    }
}
