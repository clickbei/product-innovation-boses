using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace BosesApp.Core.Data;

/// <summary>
/// Helper class to handle database migrations and schema updates
/// Ensures database schema is up-to-date without losing data
/// </summary>
public static class DatabaseMigrationHelper
{
    /// <summary>
    /// Ensures the database schema is up-to-date
    /// Adds missing columns if they don't exist
    /// </summary>
    public static async Task EnsureSchemaUpToDateAsync(BosesDbContext dbContext)
    {
        try
        {
            Debug.WriteLine("[Database] Checking database schema...");

            // Check if PreferredLanguage column exists
            var hasPreferredLanguageColumn = await CheckColumnExistsAsync(dbContext, "UserProfiles", "PreferredLanguage");

            if (!hasPreferredLanguageColumn)
            {
                Debug.WriteLine("[Database] PreferredLanguage column missing, adding it...");
                await AddPreferredLanguageColumnAsync(dbContext);
                Debug.WriteLine("[Database] ✅ PreferredLanguage column added successfully");
            }
            else
            {
                Debug.WriteLine("[Database] ✅ Schema is up-to-date");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Database] ⚠️ Schema check failed: {ex.Message}");
            Debug.WriteLine("[Database] Database will be recreated on next run");
        }
    }

    private static async Task<bool> CheckColumnExistsAsync(BosesDbContext dbContext, string tableName, string columnName)
    {
        try
        {
            var sql = $"SELECT COUNT(*) FROM pragma_table_info('{tableName}') WHERE name='{columnName}'";
            var connection = dbContext.Database.GetDbConnection();

            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            var result = await command.ExecuteScalarAsync();
            await connection.CloseAsync();

            return Convert.ToInt32(result) > 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Database] Error checking column: {ex.Message}");
            return false;
        }
    }

    private static async Task AddPreferredLanguageColumnAsync(BosesDbContext dbContext)
    {
        try
        {
            // Add PreferredLanguage column with default value 0 (English)
            var sql = "ALTER TABLE UserProfiles ADD COLUMN PreferredLanguage INTEGER NOT NULL DEFAULT 0";
            await dbContext.Database.ExecuteSqlRawAsync(sql);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Database] Error adding column: {ex.Message}");
            throw;
        }
    }
}
