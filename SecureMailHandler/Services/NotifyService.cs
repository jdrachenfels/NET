using Rebex.Mail;
using Rebex.Mime.Headers;
using System.Net;

namespace SecureMailHandler.Services;

public interface INotifyService
{
    Task SendAsync(string from, string recipient, string link, string senderName, string subject);
}

public sealed class NotifyService : INotifyService
{
    private readonly ISmtpSender _smtp;
    public NotifyService(ISmtpSender smtp) => _smtp = smtp;

    public async Task SendAsync(string from, string recipient, string link, string senderName, string subject)
    {
        var note = new MailMessage
        {
            From = new MailAddress(from),
            Subject = "Sichere Nachricht zur Abholung"
        };
        note.To.Add(recipient);

        var safeSender = WebUtility.HtmlEncode(senderName);
        var safeSubject = WebUtility.HtmlEncode(subject);
        var safeLink = WebUtility.HtmlEncode(link);

        note.BodyText = $"Eine Nachricht von {senderName} mit Betreff \"{subject}\" steht zur Abholung bereit.\n{link}\n";
        note.BodyHtml =
$@"<html><body style=""font-family:sans-serif"">
<p>Eine Nachricht von <b>{safeSender}</b> mit Betreff ""{safeSubject}"" steht zur Abholung bereit.</p>
<p><a href=""{safeLink}"" style=""display:inline-block;padding:10px 14px;text-decoration:none;border:1px solid #ccc;border-radius:6px"">Nachricht im Portal öffnen</a></p>
</body></html>";

        await _smtp.SendAsync(note);
    }
}
