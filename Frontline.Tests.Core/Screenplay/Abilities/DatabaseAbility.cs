using Frontline.Tests.Core.Screenplay.Core;
using Microsoft.Data.SqlClient;

namespace Frontline.Tests.Core.Screenplay.Abilities;

/// <summary>SQL Server database ability: manages connection lifecycle and parameterised query execution.</summary>
public class DatabaseAbility : IAbility
{
    private SqlConnection? _connection;

    public string AbilityName => "DatabaseAbility";

    public SqlConnection Connection => _connection
        ?? throw new InvalidOperationException("Database connection not initialized. Call InitializeAsync first.");

    /// <summary>Opens a connection to SQL Server.</summary>
    public async Task InitializeAsync(string connectionString)
    {
        _connection = new SqlConnection(connectionString);
        await _connection.OpenAsync();
    }

    /// <summary>Executes a parameterised non-query (INSERT, UPDATE, DELETE). Returns rows affected.</summary>
    public async Task<int> ExecuteAsync(string sql, params SqlParameter[] parameters)
    {
        await using var cmd = new SqlCommand(sql, Connection);
        cmd.Parameters.AddRange(parameters);
        return await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>Executes a parameterised scalar query. Returns the first column of the first row.</summary>
    public async Task<T?> ScalarAsync<T>(string sql, params SqlParameter[] parameters)
    {
        await using var cmd = new SqlCommand(sql, Connection);
        cmd.Parameters.AddRange(parameters);
        var result = await cmd.ExecuteScalarAsync();
        return result is DBNull or null ? default : (T)result;
    }

    /// <summary>Closes and disposes the SQL connection.</summary>
    public async Task CloseAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}
