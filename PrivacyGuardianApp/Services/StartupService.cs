using System.IO;
using Microsoft.Win32;
using PrivacyGuardian.Models;

namespace PrivacyGuardian.Services;

public sealed class StartupService(ILogRepository logs) : IStartupService
{
    private static readonly string[] RunKeys =
    [
        @"Software\Microsoft\Windows\CurrentVersion\Run",
        @"Software\Microsoft\Windows\CurrentVersion\RunOnce"
    ];

    public async Task<IReadOnlyList<StartupEntry>> GetStartupEntriesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var entries = new List<StartupEntry>();
        foreach (var keyPath in RunKeys)
        {
            AddRegistryEntries(entries, Registry.CurrentUser, keyPath, "Registry Startup");
            AddRegistryEntries(entries, Registry.LocalMachine, keyPath, "Registry Startup");
        }

        AddStartupFolder(entries, Environment.GetFolderPath(Environment.SpecialFolder.Startup));
        AddStartupFolder(entries, Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup));
        entries.AddRange(await GetScheduledTasksAsync(cancellationToken));
        return entries;
    }

    public async Task DisableAsync(StartupEntry entry, CancellationToken cancellationToken)
    {
        if (entry.Source == "Startup Folder" && File.Exists(entry.Location))
        {
            File.Move(entry.Location, entry.Location + ".disabled", true);
        }
        else if (entry.Source == "Registry Startup")
        {
            var parts = entry.Location.Split('|');
            var hive = parts[0] == "HKCU" ? Registry.CurrentUser : Registry.LocalMachine;
            using var key = hive.OpenSubKey(parts[1], true);
            key?.DeleteValue(entry.Name, false);
        }
        else if (entry.Source == "Task Scheduler")
        {
            await RunProcessAsync("schtasks", $"/Change /TN \"{entry.Location}\" /Disable", cancellationToken);
        }

        await logs.AddAsync(new Models.LogEntry { Category = "Startup", Message = "Startup entry disabled", Details = entry.Name }, cancellationToken);
    }

    private static void AddRegistryEntries(List<StartupEntry> entries, RegistryKey hive, string keyPath, string source)
    {
        using var key = hive.OpenSubKey(keyPath);
        if (key is null)
        {
            return;
        }

        var hiveName = hive.Name.Contains("CURRENT_USER", StringComparison.OrdinalIgnoreCase) ? "HKCU" : "HKLM";
        foreach (var name in key.GetValueNames())
        {
            entries.Add(new StartupEntry
            {
                Name = name,
                Command = key.GetValue(name)?.ToString() ?? string.Empty,
                Source = source,
                IsEnabled = true,
                Location = $"{hiveName}|{keyPath}"
            });
        }
    }

    private static void AddStartupFolder(List<StartupEntry> entries, string folder)
    {
        if (!Directory.Exists(folder))
        {
            return;
        }

        foreach (var file in Directory.GetFiles(folder))
        {
            entries.Add(new StartupEntry
            {
                Name = Path.GetFileNameWithoutExtension(file),
                Command = file,
                Source = "Startup Folder",
                IsEnabled = !file.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase),
                Location = file
            });
        }
    }

    private static async Task<IReadOnlyList<StartupEntry>> GetScheduledTasksAsync(CancellationToken cancellationToken)
    {
        var output = await RunProcessAsync("schtasks", "/Query /FO LIST /V", cancellationToken).ConfigureAwait(false);
        return await Task.Run<IReadOnlyList<StartupEntry>>(() =>
        {
        var entries = new List<StartupEntry>();
        foreach (var block in output.Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries))
        {
            var taskName = Field(block, "TaskName");
            var taskToRun = Field(block, "Task To Run");
            var status = Field(block, "Status");
            if (string.IsNullOrWhiteSpace(taskName) || string.IsNullOrWhiteSpace(taskToRun) || taskToRun.Equals("N/A", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            entries.Add(new StartupEntry
            {
                Name = Path.GetFileName(taskName),
                Source = "Task Scheduler",
                Command = taskToRun,
                IsEnabled = !status.Contains("Disabled", StringComparison.OrdinalIgnoreCase),
                Location = taskName
            });
        }

        return entries;
        }, cancellationToken).ConfigureAwait(false);
    }

    private static string Field(string block, string label)
    {
        var line = block.Split('\n').FirstOrDefault(l => l.TrimStart().StartsWith(label + ":", StringComparison.OrdinalIgnoreCase));
        return line is null ? string.Empty : line[(line.IndexOf(':') + 1)..].Trim();
    }

    private static async Task<string> RunProcessAsync(string fileName, string arguments, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(8));
        var startInfo = new System.Diagnostics.ProcessStartInfo(fileName, arguments)
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = System.Diagnostics.Process.Start(startInfo);
        if (process is null)
        {
            return string.Empty;
        }

        var output = await process.StandardOutput.ReadToEndAsync(timeout.Token).ConfigureAwait(false);
        var error = await process.StandardError.ReadToEndAsync(timeout.Token).ConfigureAwait(false);
        await process.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(output) ? error : output;
    }
}
