using System.IO;
using PrivacyGuardian.Core;
using PrivacyGuardian.Models;

namespace PrivacyGuardian.Services;

public sealed class UsbMonitorService(ILogRepository logs, INotificationService notifications) : IUsbMonitorService
{
    private readonly HashSet<string> _knownRemovableDrives = [];
    private bool _started;

    public event EventHandler<UsbDeviceEvent>? DeviceChanged;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_started)
        {
            return Task.CompletedTask;
        }

        _started = true;
        foreach (var drive in GetRemovableDrives())
        {
            _knownRemovableDrives.Add(drive.Name);
        }

        _ = Task.Run(() => MonitorLoopAsync(cancellationToken), cancellationToken);
        return Task.CompletedTask;
    }

    private async Task MonitorLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var current = GetRemovableDrives().ToDictionary(d => d.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var drive in current.Values.Where(d => !_knownRemovableDrives.Contains(d.Name)))
            {
                _knownRemovableDrives.Add(drive.Name);
                Raise(new UsbDeviceEvent
                {
                    Action = "Connected",
                    Vendor = drive.VolumeLabel,
                    Product = drive.DriveFormat,
                    SerialNumber = drive.Name,
                    Severity = Severity.Warning
                });
            }

            foreach (var removed in _knownRemovableDrives.Where(name => !current.ContainsKey(name)).ToArray())
            {
                _knownRemovableDrives.Remove(removed);
                Raise(new UsbDeviceEvent { Action = "Removed", SerialNumber = removed, Severity = Severity.Information });
            }

            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        }
    }

    private static IEnumerable<DriveInfo> GetRemovableDrives() =>
        DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Removable && d.IsReady);

    private void Raise(UsbDeviceEvent deviceEvent)
    {
        _ = logs.AddAsync(new LogEntry { Category = "USB", Severity = deviceEvent.Severity, Message = deviceEvent.Action, Details = $"{deviceEvent.Vendor} {deviceEvent.Product} {deviceEvent.SerialNumber}" }, CancellationToken.None);
        notifications.Show(AppText.Usb, $"{deviceEvent.Action}: {deviceEvent.Vendor} {deviceEvent.Product}", deviceEvent.Severity);
        DeviceChanged?.Invoke(this, deviceEvent);
    }
}
