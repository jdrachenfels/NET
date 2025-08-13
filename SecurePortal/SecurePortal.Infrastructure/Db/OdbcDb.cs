using System.Data;
using System.Data.Odbc;
using System.Text.RegularExpressions;

namespace SecurePortal.Infrastructure.Db;

public sealed class OdbcDb : ISimpleDb
{
    private readonly string _connStr;
    private OdbcConnection? _con;

    public OdbcDb(string connStr) => _connStr = connStr;

    public async Task OpenAsync()
    {
        _con = new OdbcConnection(_connStr);
        await _con.OpenAsync();
    }

    public async Task<int> ExecuteAsync(string sql, IDictionary<string, object?> args)
    {
        using var cmd = new OdbcCommand(Normalize(sql, args, out var order), _con);
        foreach (var name in order)
        {
            var val = args.TryGetValue(name, out var v) ? v : null;
            cmd.Parameters.Add(new OdbcParameter { Value = val ?? DBNull.Value });
        }
        cmd.Prepare();
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<T?> QuerySingleAsync<T>(string sql, IDictionary<string, object?> args, Func<IDataReader, T> map)
    {
        using var cmd = new OdbcCommand(Normalize(sql, args, out var order), _con);
        foreach (var name in order)
        {
            var val = args.TryGetValue(name, out var v) ? v : null;
            cmd.Parameters.Add(new OdbcParameter { Value = val ?? DBNull.Value });
        }
        cmd.Prepare();
        using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
        if (await rd.ReadAsync()) return map(rd);
        return default;
    }

    // Replace @name by ?, return order of parameters
    private static string Normalize(string sql, IDictionary<string, object?> args, out List<string> order)
    {
        var ord = new List<string>();
        var rx = new Regex(@"@[a-zA-Z_][a-zA-Z0-9_]*");
        string result = rx.Replace(sql, m => { ord.Add(m.Value.Substring(1)); return "?"; });
        order = ord;
        return result;
    }


    public ValueTask DisposeAsync()
    {
        _con?.Dispose();
        return ValueTask.CompletedTask;
    }
}
