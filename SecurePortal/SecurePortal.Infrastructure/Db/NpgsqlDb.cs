using Npgsql;
using System.Data;

namespace SecurePortal.Infrastructure.Db;

public sealed class NpgsqlDb : ISimpleDb
{
    private readonly string _connStr;
    private NpgsqlConnection? _con;

    public NpgsqlDb(string connStr) => _connStr = connStr;

    public async Task OpenAsync()
    {
        _con = new NpgsqlConnection(_connStr);
        await _con.OpenAsync();
    }

    public async Task<int> ExecuteAsync(string sql, IDictionary<string, object?> args)
    {
        using var cmd = new NpgsqlCommand(sql, _con);
        foreach (var kv in args)
            cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
        // await cmd.PrepareAsync();
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<T?> QuerySingleAsync<T>(string sql, IDictionary<string, object?> args, Func<IDataReader, T> map)
    {
        using var cmd = new NpgsqlCommand(sql, _con);
        foreach (var kv in args)
            cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
        
        // await cmd.PrepareAsync();
        using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
        
        if (await rd.ReadAsync()) return map(rd);
        return default;
    }

    public ValueTask DisposeAsync()
    {
        _con?.Dispose();
        return ValueTask.CompletedTask;
    }
}
