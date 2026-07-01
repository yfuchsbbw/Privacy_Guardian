namespace PrivacyGuardian.Helpers;

public static class FormatHelper
{
    public static string Percent(double value) => $"{Math.Clamp(value, 0, 100):0}%";

    public static string Bytes(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        double size = bytes;
        var suffix = 0;
        while (size >= 1024 && suffix < suffixes.Length - 1)
        {
            size /= 1024;
            suffix++;
        }

        return $"{size:0.##} {suffixes[suffix]}";
    }

    public static string BytesPerSecond(double bytes) => $"{Bytes((long)Math.Max(0, bytes))}/s";
}
