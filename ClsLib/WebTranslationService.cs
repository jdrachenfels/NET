using ClsLib;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace Web.Services;

public class WebTranslationService
{
    private readonly ClsTranslationService _translator;
    private readonly IHttpContextAccessor _http;

    public WebTranslationService(IConfiguration config, IHttpContextAccessor http)
    {
        var transDir = config["Translation:DefaultDirectory"] ?? "translate";
        var modDir = config["Translation:OverrideDirectory"] ?? "data/translate";

        _translator = new ClsTranslationService(transDir, modDir);
        _http = http;
    }

    public string T(string key)
    {
        var ctx = _http.HttpContext!;
        var lang = ctx.Session.GetString("lang") ?? "en";
        //var langBytes = ctx.Session.TryGetValue("lang", out var value) ? value : null;
        //var lang = langBytes is null ? "en" : System.Text.Encoding.UTF8.GetString(langBytes);
        var page = ctx.Request.Path;

        return _translator.Translate(key, lang, page);
    }
}