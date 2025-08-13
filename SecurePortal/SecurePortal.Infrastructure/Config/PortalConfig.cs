using ClsLib;

namespace SecurePortal.Infrastructure.Config;

public sealed class PortalConfig
{
    public string Root { get; }
    public string BaseUrl { get; }
    public string DefaultCulture { get; }
    public string CookieName { get; }
    public int SessionMinutes { get; }

    public string Provider { get; }
    public string Host { get; }
    public int Port { get; }
    public string Database { get; }
    public string Username { get; }
    public string Password { get; }
    public string SslMode { get; }
    public int PoolMax { get; }
    public int PoolMin { get; }
    public int TimeoutSeconds { get; }
    public string? OdbcConnStr { get; }

    public static PortalConfig FromIni(string? path = null)
    {
        path ??= Environment.GetEnvironmentVariable("SECURE_PORTAL_INI") ?? "/etc/secure-portal.ini";
        var ini = new ClsIniFile(path);

        return new PortalConfig(
            ini.ReadINI("general","root","/workdir/workspace/secure_mail"),
            ini.ReadINI("general","base_url","https://secure.example"),
            ini.ReadINI("general","default_culture","de"),
            ini.ReadINI("general","cookie_name","SECUREPORTAL"),
            int.Parse(ini.ReadINI("general","session_minutes","30")),
            provider: ini.ReadINI("db","provider","npgsql"),
            host:     ini.ReadINI("db","host","pg"),
            port:     int.Parse(ini.ReadINI("db","port","5432")),
            database: ini.ReadINI("db","database","secure"),
            username: ini.ReadINI("db","username","secure"),
            password: ini.ReadINI("db","password","change_me"),
            sslmode:  ini.ReadINI("db","sslmode","Prefer"),
            poolMax:  int.Parse(ini.ReadINI("db","pool_max","100")),
            poolMin:  int.Parse(ini.ReadINI("db","pool_min","0")),
            timeout:  int.Parse(ini.ReadINI("db","timeout_seconds","15")),
            odbc:     ini.ReadINI("db","odbc_connstr","Driver=PostgreSQL Unicode;Server=pg;Port=5432;Database=secure;Uid=secure;Pwd=change_me;SSLmode=prefer;")
        );
    }

    private PortalConfig(string root, string baseUrl, string defaultCulture, string cookieName, int sessionMinutes,
        string provider, string host, int port, string database, string username, string password, string sslmode,
        int poolMax, int poolMin, int timeout, string odbc)
    { Root=root; BaseUrl=baseUrl; DefaultCulture=defaultCulture; CookieName=cookieName; SessionMinutes=sessionMinutes;
      Provider=provider; Host=host; Port=port; Database=database; Username=username; Password=password; SslMode=sslmode;
      PoolMax=poolMax; PoolMin=poolMin; TimeoutSeconds=timeout; OdbcConnStr=odbc; }
}
