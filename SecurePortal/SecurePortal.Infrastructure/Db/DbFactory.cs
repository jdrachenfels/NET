using SecurePortal.Infrastructure.Config;

namespace SecurePortal.Infrastructure.Db;

public static class DbFactory
{
    public static ISimpleDb Create(PortalConfig cfg)
        => cfg.Provider.Equals("odbc", StringComparison.OrdinalIgnoreCase)
           ? new OdbcDb(cfg.OdbcConnStr!)
           : new NpgsqlDb($"Host={cfg.Host};Port={cfg.Port};Database={cfg.Database};Username={cfg.Username};Password={cfg.Password};SslMode={cfg.SslMode};Maximum Pool Size={cfg.PoolMax};Minimum Pool Size={cfg.PoolMin};Timeout={cfg.TimeoutSeconds};Command Timeout={cfg.TimeoutSeconds}");
}
