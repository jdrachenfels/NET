using System.Text.Json;
using System.Text.RegularExpressions;

namespace ClsLib;

public sealed class ClsTranslationService
{
    private const string DEFAULT_LANG = "en";

    private readonly string _transDir;
    private readonly string _transModDir;
    private readonly bool _fallbackToDefaultLanguage;

    public ClsTranslationService(
        string transDir,
        string transModDir,
        bool fallbackToDefaultLanguage = true)
    {
        _transDir = string.IsNullOrWhiteSpace(transDir) ? "translate" : transDir;
        _transModDir = string.IsNullOrWhiteSpace(transModDir) ? "data/translate" : transModDir;
        _fallbackToDefaultLanguage = fallbackToDefaultLanguage;
    }

    public string Translate(string transKey, string lang, string page)
    {
        if (string.IsNullOrWhiteSpace(transKey))
            return string.Empty;

        lang = NormalizeLang(lang);
        page = NormalizePage(page);

        // 1. normale Suche (gewählte Sprache)
        var value = Resolve(transKey, lang, page);
        if (!string.IsNullOrEmpty(value))
            return value;

        // 2. Fallback auf Default-Sprache (optional)
        if (_fallbackToDefaultLanguage && lang != DEFAULT_LANG)
        {
            value = Resolve(transKey, DEFAULT_LANG, page);
            if (!string.IsNullOrEmpty(value))
                return value;
        }

        // 3. finaler Fallback: Key selbst
        return transKey;
    }

    private string? Resolve(string transKey, string lang, string page)
    {
        var files = new[]
        {
            Path.Combine(_transModDir, $"{lang}.{page}.json"),
            Path.Combine(_transModDir, $"{lang}.json"),
            Path.Combine(_transDir, $"{lang}.{page}.json"),
            Path.Combine(_transDir, $"{lang}.json")
        };

        foreach (var file in files)
        {
            var value = TryReadValue(file, transKey);
            if (!string.IsNullOrEmpty(value))
                return value;
        }

        return null;
    }

    public string NormalizePage(string? page)
    {
        if (string.IsNullOrWhiteSpace(page))
            return "default";

        var file = Path.GetFileNameWithoutExtension(page).ToLowerInvariant();

        // nur a-z und 0-9
        file = Regex.Replace(file, "[^a-z0-9]", "");

        return string.IsNullOrWhiteSpace(file) ? "default" : file;
    }

    public string NormalizeLang(string? lang)
    {
        if (string.IsNullOrWhiteSpace(lang))
            return DEFAULT_LANG;

        lang = lang.Trim().ToLowerInvariant();

        // nur a-z und 0-9
        lang = Regex.Replace(lang, "[^a-z0-9]", "");

        return string.IsNullOrWhiteSpace(lang) ? DEFAULT_LANG : lang;
    }

    private string? TryReadValue(string fileName, string transKey)
    {
        if (!File.Exists(fileName))
            return null;

        try
        {
            var json = File.ReadAllText(fileName);
            var values = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            if (values == null)
                return null;

            return values.TryGetValue(transKey, out var value) ? value : null;
        }
        catch
        {
            // später optional Logging via ClsSyslog
            return null;
        }
    }
}