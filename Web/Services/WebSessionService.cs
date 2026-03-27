using System.Text.Json;
using Web.Models;

namespace Web.Services;

public class WebSessionService
{
    private const string SESSION_KEY = "user";

    private readonly IHttpContextAccessor _http;

    public WebSessionService(IHttpContextAccessor http)
    {
        _http = http;
    }

    public void SetUser(WebUserSession user)
    {
        var json = JsonSerializer.Serialize(user);
        _http.HttpContext!.Session.SetString(SESSION_KEY, json);
    }

    public WebUserSession? GetUser()
    {
        var json = _http.HttpContext!.Session.GetString(SESSION_KEY);

        if (string.IsNullOrWhiteSpace(json))
            return null;

        return JsonSerializer.Deserialize<WebUserSession>(json);
    }

    public void Clear()
    {
        _http.HttpContext!.Session.Remove(SESSION_KEY);
    }
}