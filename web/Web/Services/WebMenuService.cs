using Web.Models;

namespace Web.Services;

public class WebMenuService
{
    private readonly IHttpContextAccessor _http;
    private readonly WebTranslationService _translation;
    private readonly WebSessionService _session;

    public WebMenuService(
        IHttpContextAccessor http,
        WebTranslationService translation,
        WebSessionService session)
    {
        _http = http;
        _translation = translation;
        _session = session;
    }

    public List<NavMenuItem> GetMenu()
    {
        var path = (_http.HttpContext?.Request.Path.Value ?? "").ToLowerInvariant();

        var menu = new List<NavMenuItem>
        {
            new NavMenuItem
            {
                Key = "home",
                TextKey = "menu.home",
                Url = "/home",
                Icon = "⌂",
                IsVisible = HasPermission("home.view")
            },

            new NavMenuItem
            {
                Key = "routing",
                TextKey = "menu.routing",
                Url = "#",
                Icon = "↪",
                IsVisible = HasPermission("routing.view"),
                Children = new List<NavMenuItem>
                {
                    new NavMenuItem
                    {
                        Key = "domain",
                        TextKey = "menu.domain",
                        Url = "/domain",
                        IsVisible = HasPermission("domain.view")
                    }
                }
            }
        };

        MarkActive(menu, path);

        // unsichtbare Kinder raus, dann leere Hauptpunkte raus
        foreach (var item in menu)
        {
            item.Children = item.Children.Where(x => x.IsVisible).ToList();
        }

        return menu
            .Where(x => x.IsVisible && (x.Children.Count > 0 || x.Url != "#"))
            .ToList();
    }

    private void MarkActive(List<NavMenuItem> items, string path)
    {
        foreach (var item in items)
        {
            item.IsActive = item.Url != "#" && path.StartsWith(item.Url.ToLowerInvariant());

            foreach (var child in item.Children)
            {
                child.IsActive = child.Url != "#" && path.StartsWith(child.Url.ToLowerInvariant());
            }

            if (item.Children.Any(x => x.IsActive))
            {
                item.IsActive = true;
            }
        }
    }

    private bool HasPermission(string permission)
    {
        var user = _session.GetUser();

        if (user == null)
            return false;

        return user.Permissions.Contains(permission);
    }

    public string TT(string key) => _translation.T(key);
}