using Microsoft.Data.Sqlite;
using PrivacyGuardian.Core;
using PrivacyGuardian.Models;
using PrivacyGuardian.Services;

namespace PrivacyGuardian.Database;

public sealed class SqliteLogRepository : ILogRepository
{
    private readonly string _connectionString = $"Data Source={DatabasePathProvider.GetDatabasePath()}";

    public async Task AddAsync(LogEntry entry, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Logs (Timestamp, Severity, Category, Message, Details)
            VALUES ($timestamp, $severity, $category, $message, $details);
            """;
        command.Parameters.AddWithValue("$timestamp", entry.Timestamp.ToString("O"));
        command.Parameters.AddWithValue("$severity", entry.Severity.ToString());
        command.Parameters.AddWithValue("$category", entry.Category);
        command.Parameters.AddWithValue("$message", entry.Message);
        command.Parameters.AddWithValue("$details", entry.Details);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LogEntry>> QueryAsync(DateTimeOffset? from, DateTimeOffset? to, Severity? severity, string? category, CancellationToken cancellationToken)
    {
        var filters = new List<string>();
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();

        if (from is not null)
        {
            filters.Add("Timestamp >= $from");
            command.Parameters.AddWithValue("$from", from.Value.ToString("O"));
        }

        if (to is not null)
        {
            filters.Add("Timestamp <= $to");
            command.Parameters.AddWithValue("$to", to.Value.ToString("O"));
        }

        if (severity is not null)
        {
            filters.Add("Severity = $severity");
            command.Parameters.AddWithValue("$severity", severity.Value.ToString());
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            filters.Add("Category = $category");
            command.Parameters.AddWithValue("$category", category);
        }

        command.CommandText = $"""
            SELECT Id, Timestamp, Severity, Category, Message, Details
            FROM Logs
            {(filters.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", filters))}
            ORDER BY Timestamp DESC
            LIMIT 500;
            """;

        var entries = new List<LogEntry>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            entries.Add(new LogEntry
            {
                Id = reader.GetInt64(0),
                Timestamp = DateTimeOffset.Parse(reader.GetString(1)),
                Severity = Enum.TryParse<Severity>(reader.GetString(2), out var parsed) ? parsed : Severity.Information,
                Category = reader.GetString(3),
                Message = reader.GetString(4),
                Details = reader.GetString(5)
            });
        }

        return entries;
    }
}
