using ClsLib;

namespace SecureMailHandler.Config;

public sealed class HandlerConfig
{
    public string Root { get; }
    public string SmtpHost { get; }
    public int SmtpPort { get; }
    public string From { get; }
    public bool EnableDirect { get; }

    public static HandlerConfig FromIni(string? path=null)
    {
        path ??= Environment.GetEnvironmentVariable("SECURE_HANDLER_INI") ?? "/etc/secure-handler.ini";
        var ini = new ClsIniFile(path); // ReadINI schreibt Defaults zurück

        return new HandlerConfig(
            ini.ReadINI("general","root","/workdir/workspace/secure_mail"),
            ini.ReadINI("general","smtp_host","127.0.0.1"),
            int.Parse(ini.ReadINI("general","smtp_port","25")),
            ini.ReadINI("general","from","no-reply@drachenfels.de"),
            bool.Parse(ini.ReadINI("policy","enable_direct_delivery","false"))
        );
    }

    private HandlerConfig(string root, string host, int port, string from, bool direct)
    { Root=root; SmtpHost=host; SmtpPort=port; From=from; EnableDirect=direct; }
}
