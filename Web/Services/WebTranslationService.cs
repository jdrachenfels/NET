using ClsLib;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Web.Services;
using Web.Models;


namespace Web.Services;

public class WebTranslationService
{
    private readonly ClsTranslationService _translator;
    private readonly IHttpContextAccessor _http;
    private readonly WebSessionService _session;

    public WebTranslationService(
        IConfiguration config,
        IHttpContextAccessor http,
        WebSessionService session)
    {
        var transDir = config["Translation:DefaultDirectory"] ?? "translate";
        var modDir = config["Translation:OverrideDirectory"] ?? "data/translate";

        _translator = new ClsTranslationService(transDir, modDir);

        _http = http;
        _session = session;
    }

    public string T(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        var ctx = _http.HttpContext;

        // Sprache aus Session holen
        var user = _session.GetUser();
        var lang = user?.Lang ?? "en";

        // aktuelle Seite bestimmen
        var page = ctx?.Request.Path.Value ?? "";

        return _translator.Translate(key, lang, page);
    }
}