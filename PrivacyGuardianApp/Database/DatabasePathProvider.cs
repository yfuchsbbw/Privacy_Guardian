using System.IO;
using PrivacyGuardian.Core;

namespace PrivacyGuardian.Database;

public static class DatabasePathProvider
{
    public static string GetDatabasePath()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Constants.ProductName);

        return Path.Combine(folder, Constants.DatabaseFileName);
    }
}
