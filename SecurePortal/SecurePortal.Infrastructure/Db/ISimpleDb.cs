using System.Data;

namespace SecurePortal.Infrastructure.Db;

public interface ISimpleDb : IAsyncDisposable
{
    Task OpenAsync();
    Task<int> ExecuteAsync(string sql, IDictionary<string, object?> args);
    Task<T?> QuerySingleAsync<T>(string sql, IDictionary<string, object?> args, Func<IDataReader, T> map);
}
