using System.Runtime.InteropServices;
using Microsoft.Win32;
using PrivacyGuardian.Core;
using PrivacyGuardian.Models;

namespace PrivacyGuardian.Services;

public sealed class PrivacyMonitorService(ILogRepository logs, INotificationService notifications) : IPrivacyMonitorService
{
    private readonly List<PrivacyEvent> _events = [];
    private readonly HashSet<string> _activeResources = [];
    private uint _lastClipboardSequence;
    private bool _started;

    public event EventHandler<PrivacyEvent>? PrivacyEventDetected;

    public Task<IReadOnlyList<PrivacyEvent>> GetCurrentEventsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_events)
        {
            return Task.FromResult<IReadOnlyList<PrivacyEvent>>(_events.OrderByDescending(e => e.Timestamp).Take(100).ToList());
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_started)
        {
            return Task.CompletedTask;
        }

        _started = true;
        _lastClipboardSequence = GetClipboardSequenceNumber();
        _ = Task.Run(() => MonitorLoopAsync(cancellationToken), cancellationToken);
        return Task.CompletedTask;
    }

    private async Task MonitorLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            DetectCapabilityUse("Camera", @"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam");
            DetectCapabilityUse("Microphone", @"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\microphone");
            DetectCapabilityUse("Location", @"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location");
            DetectClipboardChange();
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        }
    }

    private void DetectCapabilityUse(string resource, string keyPath)
    {
        using var key = Registry.CurrentUser.OpenSubKey(keyPath);
        if (key is null)
        {
            return;
        }

        foreach (var subKeyName in key.GetSubKeyNames())
        {
            using var appKey = key.OpenSubKey(subKeyName);
            var stop = appKey?.GetValue("LastUsedTimeStop");
            var start = appKey?.GetValue("LastUsedTimeStart");
            var identity = subKeyName.Replace("#", "\\");
            var eventKey = $"{resource}:{identity}";
            var active = start is long startValue && startValue > 0 && stop is long stopValue && stopValue == 0;

            if (active && _activeResources.Add(eventKey))
            {
                Raise(new PrivacyEvent { Application = identity, Resource = resource, State = "Started", Severity = Severity.Warning });
            }
            else if (!active && _activeResources.Remove(eventKey))
            {
                Raise(new PrivacyEvent { Application = identity, Resource = resource, State = "Stopped", Severity = Severity.Information });
            }
        }
    }

    private void DetectClipboardChange()
    {
        var sequence = GetClipboardSequenceNumber();
        if (sequence == 0 || sequence == _lastClipboardSequence)
        {
            return;
        }

        _lastClipboardSequence = sequence;
        Raise(new PrivacyEvent
        {
            Application = "Windows Clipboard",
            Resource = "Clipboard",
            State = "Changed",
            Severity = Severity.Information
        });
    }

    private void Raise(PrivacyEvent privacyEvent)
    {
        lock (_events)
        {
            _events.Add(privacyEvent);
        }

        _ = logs.AddAsync(new LogEntry
        {
            Category = "Privacy",
            Severity = privacyEvent.Severity,
            Message = $"{privacyEvent.Resource} {privacyEvent.State}",
            Details = privacyEvent.Application
        }, CancellationToken.None);

        notifications.Show(AppText.Privacy, $"{privacyEvent.Application}: {privacyEvent.Resource} {privacyEvent.State}", privacyEvent.Severity);
        PrivacyEventDetected?.Invoke(this, privacyEvent);
    }

    [DllImport("user32.dll")]
    private static extern uint GetClipboardSequenceNumber();
}
