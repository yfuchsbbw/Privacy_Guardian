using System.IO;
using PrivacyGuardian.Core;
using PrivacyGuardian.Models;

namespace PrivacyGuardian.Services;

public sealed class FileActivityService(ILogRepository logs) : IFileActivityService
{
    private readonly List<FileSystemWatcher> _watchers = [];
    private bool _started;

    public event EventHandler<FileActivityEvent>? ActivityDetected;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_started)
        {
            return Task.CompletedTask;
        }

        _started = true;
        var folders = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")
        };

        foreach (var folder in folders.Where(Directory.Exists).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var watcher = new FileSystemWatcher(folder)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.CreationTime
            };
            watcher.Created += (_, e) => Raise(e.FullPath, string.Empty, "Created");
            watcher.Changed += (_, e) => Raise(e.FullPath, string.Empty, "Modified");
            watcher.Deleted += (_, e) => Raise(e.FullPath, string.Empty, "Deleted");
            watcher.Renamed += (_, e) => Raise(e.FullPath, e.OldFullPath, "Renamed");
            _watchers.Add(watcher);
        }

        cancellationToken.Register(() =>
        {
            foreach (var watcher in _watchers)
            {
                watcher.Dispose();
            }
        });

        return Task.CompletedTask;
    }

    private void Raise(string path, string oldPath, string action)
    {
        var entry = new FileActivityEvent { Path = path, OldPath = oldPath, Action = action, Severity = action == "Deleted" ? Severity.Warning : Severity.Information };
        _ = logs.AddAsync(new LogEntry { Category = "File Activity", Severity = entry.Severity, Message = action, Details = string.IsNullOrWhiteSpace(oldPath) ? path : $"{oldPath} -> {path}" }, CancellationToken.None);
        ActivityDetected?.Invoke(this, entry);
    }
}
