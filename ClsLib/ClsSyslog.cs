using System.Net.Sockets;
using System.Net;
using System.Text;

namespace ClsLib;

public enum SyslogFacility
{
    Kern=0, User=1, Mail=2, Daemon=3, Auth=4, Syslog=5, Lpr=6, News=7,
    Uucp=8, Cron=9, Authpriv=10, Ftp=11, Ntp=12, Audit=13, Alert=14, Clock=15,
    Local0=16, Local1=17, Local2=18, Local3=19, Local4=20, Local5=21, Local6=22, Local7=23
}

public enum SyslogSeverity { Emergency=0, Alert=1, Critical=2, Error=3, Warning=4, Notice=5, Info=6, Debug=7 }

public sealed class ClsSyslog : IDisposable
{
    private readonly bool _enabled;
    private readonly string _server;
    private readonly int _port;
    private readonly string _app;
    private readonly string _host;
    private readonly ProtocolType _proto;
    private readonly SyslogFacility _facility;
    private readonly SyslogSeverity _minLevel;

    private UdpClient? _udp;
    private TcpClient? _tcp;
    private NetworkStream? _tcpStream;

    public ClsSyslog(bool enabled, string server, int port, string protocol, string appname,
                     SyslogFacility facility, SyslogSeverity minLevel, string? hostname=null)
    {
        _enabled = enabled;
        _server  = server;
        _port    = port;
        _app     = string.IsNullOrWhiteSpace(appname) ? "app" : appname;
        _facility = facility;
        _minLevel = minLevel;
        _proto    = protocol?.ToLowerInvariant() == "tcp" ? ProtocolType.Tcp : ProtocolType.Udp;
        _host     = string.IsNullOrWhiteSpace(hostname) ? Dns.GetHostName() : hostname;

        if (!_enabled) return;
        if (_proto == ProtocolType.Udp)
            _udp = new UdpClient();
        else
        {
            _tcp = new TcpClient();
            _tcp.Connect(_server, _port);
            _tcpStream = _tcp.GetStream();
        }
    }

    public void Log(SyslogSeverity level, string message, string? msgId=null, string? procId=null)
    {
        if (!_enabled || level > _minLevel) return;
        var pri = ((int)_facility * 8) + (int)level; // PRI = Facility*8 + Severity
        var ts = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"); // RFC3339
        var proc = string.IsNullOrWhiteSpace(procId) ? Environment.ProcessId.ToString() : procId;
        var mid = string.IsNullOrWhiteSpace(msgId) ? "-" : msgId;

        // RFC 5424: <PRI>1 TIMESTAMP HOST APP PROCID MSGID - MSG
        var payload = $"<{pri}>1 {ts} {_host} {_app} {proc} {mid} - {message}";
        var bytes = Encoding.UTF8.GetBytes(payload);

        if (_proto == ProtocolType.Udp)
            _udp!.Send(bytes, bytes.Length, _server, _port);
        else
        {
            var line = Encoding.UTF8.GetBytes(payload + "\n");
            _tcpStream!.Write(line, 0, line.Length);
            _tcpStream.Flush();
        }
    }

    public void Dispose()
    {
        try { _tcpStream?.Dispose(); } catch { }
        try { _tcp?.Dispose(); } catch { }
        try { _udp?.Dispose(); } catch { }
    }
}
