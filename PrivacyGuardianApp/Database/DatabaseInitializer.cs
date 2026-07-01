using System.IO;
using Microsoft.Data.Sqlite;
using PrivacyGuardian.Services;

namespace PrivacyGuardian.Database;

public sealed class DatabaseInitializer : IDatabaseInitializer
{
    private readonly string _databasePath = DatabasePathProvider.GetDatabasePath();

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_databasePath)!);
        await using var connection = new SqliteConnection($"Data Source={_databasePath}");
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Logs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Timestamp TEXT NOT NULL,
                Severity TEXT NOT NULL,
                Category TEXT NOT NULL,
                Message TEXT NOT NULL,
                Details TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS IX_Logs_Timestamp ON Logs (Timestamp);
            CREATE INDEX IF NOT EXISTS IX_Logs_Category ON Logs (Category);
            CREATE INDEX IF NOT EXISTS IX_Logs_Severity ON Logs (Severity);
            """;

        await command.ExecuteNonQueryAsync();
    }
}
