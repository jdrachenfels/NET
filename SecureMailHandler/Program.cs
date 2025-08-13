using ClsLib;
using System.Text;
using System.Text.RegularExpressions;
using SecureMailHandler.Config;
using SecureMailHandler.Services;

var cfg = HandlerConfig.FromIni();
using var log = new ClsSyslog(
    enabled: true,
    server: "127.0.0.1", port: 514, protocol: "udp",
    appname: "secure-handler",
    facility: SyslogFacility.Local0, minLevel: SyslogSeverity.Info);

try
{
    // args: domain local_part primary_hostname exim_id [zulu]
    var domain = args.Length > 0 ? args[0] : "unknown";
    var local = args.Length > 1 ? args[1] : "unknown";
    var primary = args.Length > 2 ? args[2] : "unknown";
    var eximId = args.Length > 3 ? args[3] : "unknown";
    var zulu = args.Length > 4 ? args[4] : DateTime.UtcNow.ToString("yyyyMMddHHmmss'Z'");

    // Rohmail aus STDIN lesen (RFC822)
    byte[] raw;
    using (var ms = new MemoryStream()) { Console.OpenStandardInput().CopyTo(ms); raw = ms.ToArray(); }

    // Header grob extrahieren (From/To/Subject) – nur für Benachrichtigung
    var (fromHdr, toHdr, subjHdr) = ParseHeaders(raw);
    var recipient = !string.IsNullOrWhiteSpace(toHdr) ? toHdr! : $"{local}@{domain}";
    var sender = !string.IsNullOrWhiteSpace(fromHdr) ? fromHdr! : "(unbekannt)";
    var subject = !string.IsNullOrWhiteSpace(subjHdr) ? subjHdr! : "(ohne Betreff)";

    var sink = new MaildirSink();
    var smtp = new RebexSmtpSender(cfg.SmtpHost, cfg.SmtpPort);
    var notify = new NotifyService(smtp);

    // Ablage
    var path = sink.Save(raw, cfg.Root, domain, local, primary, eximId, zulu);

    // Opaque Platzhalter-ID (Dateiname) → später Portal-ID
    var link = $"{"https://secure.example"}/pickup?id={Uri.EscapeDataString(Path.GetFileName(path))}";

    await notify.SendAsync(cfg.From, recipient, link, sender, subject);

    log.Log(SyslogSeverity.Info, $"stored {path}");
    Console.WriteLine(path);
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    return 1;
}

// --- Hilfsfunktion: sehr einfache Header-Paser (unfolded) ---
static (string? From, string? To, string? Subject) ParseHeaders(byte[] raw)
{
    // nur die ersten ~128KB für Header analysieren
    var max = Math.Min(raw.Length, 128 * 1024);
    var text = Encoding.ASCII.GetString(raw, 0, max);

    // Header-Teil bis zur Leerzeile
    var sepIdx = text.IndexOf("\r\n\r\n", StringComparison.Ordinal);
    if (sepIdx < 0) sepIdx = text.IndexOf("\n\n", StringComparison.Ordinal);
    if (sepIdx > 0) text = text.Substring(0, sepIdx);

    // Folding entfernen: CRLF + WSP -> Leerzeichen
    text = text.Replace("\r\n", "\n");
    text = Regex.Replace(text, @"\n[ \t]+", " ");

    string? H(string name)
    {
        var m = Regex.Match(text, $"(?im)^\\s*{Regex.Escape(name)}\\s*:\\s*(.+)$");
        return m.Success ? m.Groups[1].Value.Trim() : null;
    }

    return (H("From"), H("To"), H("Subject"));
}
