using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using PrivacyGuardian.Core;
using PrivacyGuardian.Models;

namespace PrivacyGuardian.Services;

public sealed class UpdateService(ISettingsService settingsService, ILogRepository logs, ITrayService trayService) : BackgroundService, IUpdateService
{
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(20) };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken).ConfigureAwait(false);
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckForUpdatesAsync(stoppingToken).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromHours(6), stoppingToken).ConfigureAwait(false);
        }
    }

    public async Task CheckForUpdatesAsync(CancellationToken cancellationToken)
    {
        if (!settingsService.Current.AutoUpdateEnabled || string.IsNullOrWhiteSpace(settingsService.Current.UpdateManifestUrl))
        {
            return;
        }

        try
        {
            var manifestJson = await HttpClient.GetStringAsync(settingsService.Current.UpdateManifestUrl, cancellationToken).ConfigureAwait(false);
            var manifest = JsonSerializer.Deserialize<UpdateManifest>(manifestJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (manifest is null || string.IsNullOrWhiteSpace(manifest.Version) || string.IsNullOrWhiteSpace(manifest.InstallerUrl))
            {
                return;
            }

            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);
            if (!Version.TryParse(manifest.Version, out var remoteVersion) || remoteVersion <= currentVersion)
            {
                return;
            }

            var installerPath = await DownloadInstallerAsync(manifest, cancellationToken).ConfigureAwait(false);
            await logs.AddAsync(new LogEntry
            {
                Category = "Updater",
                Severity = Severity.Information,
                Message = "Update downloaded",
                Details = $"{manifest.Version}: {installerPath}"
            }, cancellationToken).ConfigureAwait(false);

            if (settingsService.Current.AutoInstallUpdates)
            {
                Process.Start(new ProcessStartInfo(installerPath) { UseShellExecute = true });
                trayService.ExitApplication();
            }
        }
        catch (Exception ex)
        {
            await logs.AddAsync(new LogEntry
            {
                Category = "Updater",
                Severity = Severity.Warning,
                Message = "Update check failed",
                Details = ex.Message
            }, CancellationToken.None).ConfigureAwait(false);
        }
    }

    private static async Task<string> DownloadInstallerAsync(UpdateManifest manifest, CancellationToken cancellationToken)
    {
        var updateDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Constants.ProductName,
            "Updates");

        Directory.CreateDirectory(updateDirectory);
        var installerPath = Path.Combine(updateDirectory, $"PrivacyGuardianSetup-{manifest.Version}.exe");
        var data = await HttpClient.GetByteArrayAsync(manifest.InstallerUrl, cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(manifest.Sha256))
        {
            var hash = Convert.ToHexString(SHA256.HashData(data));
            if (!hash.Equals(manifest.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Downloaded update failed SHA256 verification.");
            }
        }

        await File.WriteAllBytesAsync(installerPath, data, cancellationToken).ConfigureAwait(false);
        return installerPath;
    }
}
