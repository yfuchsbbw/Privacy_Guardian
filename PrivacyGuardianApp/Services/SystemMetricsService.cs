using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using PrivacyGuardian.Models;

namespace PrivacyGuardian.Services;

public sealed class SystemMetricsService : ISystemMetricsService
{
    private CpuTimes? _lastCpuTimes;
    private long _lastNetworkSent;
    private long _lastNetworkReceived;
    private DateTimeOffset _lastNetworkSample = DateTimeOffset.Now;

    public Task<SystemMetrics> GetMetricsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var cpu = GetCpuUsage();
        var ram = GetRamUsage();
        var disk = GetDiskUsage();
        var (upload, download) = GetNetworkRates();

        return Task.FromResult(new SystemMetrics(cpu, ram, disk, upload, download, DateTimeOffset.Now));
    }

    private double GetCpuUsage()
    {
        if (!GetSystemTimes(out var idle, out var kernel, out var user))
        {
            return 0;
        }

        var current = new CpuTimes(ToLong(idle), ToLong(kernel), ToLong(user));
        if (_lastCpuTimes is null)
        {
            _lastCpuTimes = current;
            return 0;
        }

        var previous = _lastCpuTimes.Value;
        _lastCpuTimes = current;
        var idleDelta = current.Idle - previous.Idle;
        var totalDelta = current.Kernel + current.User - previous.Kernel - previous.User;
        return totalDelta <= 0 ? 0 : (1.0 - idleDelta / (double)totalDelta) * 100;
    }

    private static double GetRamUsage()
    {
        var status = new MemoryStatus { Length = (uint)Marshal.SizeOf<MemoryStatus>() };
        return GlobalMemoryStatusEx(ref status) ? status.MemoryLoad : 0;
    }

    private static double GetDiskUsage()
    {
        var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.Name.Equals(Path.GetPathRoot(Environment.SystemDirectory), StringComparison.OrdinalIgnoreCase));
        if (drive is null || drive.TotalSize <= 0)
        {
            return 0;
        }

        return (1.0 - drive.AvailableFreeSpace / (double)drive.TotalSize) * 100;
    }

    private (double Upload, double Download) GetNetworkRates()
    {
        var sent = 0L;
        var received = 0L;
        foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces().Where(n => n.OperationalStatus == OperationalStatus.Up))
        {
            var stats = adapter.GetIPv4Statistics();
            sent += stats.BytesSent;
            received += stats.BytesReceived;
        }

        var now = DateTimeOffset.Now;
        var seconds = Math.Max(0.5, (now - _lastNetworkSample).TotalSeconds);
        var upload = Math.Max(0, (sent - _lastNetworkSent) / seconds);
        var download = Math.Max(0, (received - _lastNetworkReceived) / seconds);
        _lastNetworkSent = sent;
        _lastNetworkReceived = received;
        _lastNetworkSample = now;
        return (upload, download);
    }

    private static long ToLong(FileTime fileTime) => ((long)fileTime.HighDateTime << 32) + fileTime.LowDateTime;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemTimes(out FileTime idleTime, out FileTime kernelTime, out FileTime userTime);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatus buffer);

    [StructLayout(LayoutKind.Sequential)]
    private struct FileTime
    {
        public uint LowDateTime;
        public uint HighDateTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatus
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhys;
        public ulong AvailPhys;
        public ulong TotalPageFile;
        public ulong AvailPageFile;
        public ulong TotalVirtual;
        public ulong AvailVirtual;
        public ulong AvailExtendedVirtual;
    }

    private readonly record struct CpuTimes(long Idle, long Kernel, long User);
}
