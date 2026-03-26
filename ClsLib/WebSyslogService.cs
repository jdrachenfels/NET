using ClsLib;
using Microsoft.Extensions.Configuration;

namespace Web.Services;

public class WebSyslogService : IDisposable
{
    private readonly ClsSyslog _syslog;

    public WebSyslogService(IConfiguration config)
    {
        _syslog = new ClsSyslog(
            bool.Parse(config["Syslog:Enabled"] ?? "false"),
            config["Syslog:Server"] ?? "127.0.0.1",
            int.Parse(config["Syslog:Port"] ?? "514"),
            config["Syslog:Protocol"] ?? "udp",
            config["Syslog:AppName"] ?? "web",
            SyslogFacility.Local0,
            SyslogSeverity.Info
        );
    }

    public void Info(string msg) => _syslog.Log(SyslogSeverity.Info, msg);
    public void Error(string msg) => _syslog.Log(SyslogSeverity.Error, msg);

    public void Dispose() => _syslog.Dispose();
}