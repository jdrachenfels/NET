using Rebex.Net;
using Rebex.Mail;


namespace SecureMailHandler.Services;

public interface ISmtpSender { Task SendAsync(MailMessage msg); }

public sealed class RebexSmtpSender : ISmtpSender
{
    private readonly string _host; private readonly int _port;
    public RebexSmtpSender(string host, int port) { _host = host; _port = port; }

    public async Task SendAsync(MailMessage msg)
    {
        using var smtp = new Smtp();
        var mode = _port == 465 ? SslMode.Implicit : SslMode.Explicit; // 587/25 => STARTTLS
        smtp.Connect(_host, _port, mode);
        await Task.Run(() => smtp.Send(msg));
        smtp.Disconnect();
    }
}
