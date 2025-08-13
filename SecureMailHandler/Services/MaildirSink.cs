namespace SecureMailHandler.Services;

public interface IMaildirSink
{
    string Save(byte[] raw, string root, string domain, string local, string primary, string eximId, string zulu);
}

public sealed class MaildirSink : IMaildirSink
{
    public string Save(byte[] raw, string root, string domain, string local, string primary, string eximId, string zulu)
    {
        static string San(string s) => new string(s.Where(ch => char.IsLetterOrDigit(ch) || ch == '.' || ch == '_' || ch == '-').ToArray());
        var sDomain = San(domain); var sLocal = San(local); var sPrimary = San(primary);
        var sExim = San(eximId); var sZulu = San(zulu);
        var baseName = $"{sPrimary}__{sZulu}__{sExim}";

        var rootDir = Path.Combine(root, sDomain, sLocal);
        var tmpDir = Path.Combine(rootDir, "tmp");
        var curDir = Path.Combine(rootDir, "cur");
        Directory.CreateDirectory(tmpDir);
        Directory.CreateDirectory(curDir);

        var tmpFile = Path.Combine(tmpDir, $"{baseName}.{Random.Shared.Next()}.{Environment.ProcessId}");
        File.WriteAllBytes(tmpFile, raw);

        var final = Path.Combine(curDir, baseName);
        File.Move(tmpFile, final, overwrite: true);
        return final;
    }
}
