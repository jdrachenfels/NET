using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Web.Services;
using Web.Models;

namespace Web.Controllers;

public class AccountController : Controller
{
    private readonly WebAuthService _auth;
    private readonly WebSessionService _session;
    private readonly WebSyslogService _log;

    public AccountController(
        WebAuthService auth,
        WebSessionService session,
        WebSyslogService log)
    {
        _auth = auth;
        _session = session;
        _log = log;
    }

    // GET: / oder /default
    [HttpGet("/")]
    [HttpGet("/default")]
    public IActionResult Login()
    {
        return View();
    }

    // POST: /default
    [HttpPost("/default")]
    public async Task<IActionResult> Login(string username, string password, string lang)
    {
        var user = _auth.Authenticate(username, password);

        if (user != null)
        {
            var sessionUser = new WebUserSession
            {
                Username = username,
                Lang = string.IsNullOrWhiteSpace(lang) ? "en" : lang,
                Permissions = user.GetPermissions()
            };

            _session.SetUser(sessionUser);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username)
            };

            var identity = new ClaimsIdentity(claims, "cookie");

            await HttpContext.SignInAsync("cookie", new ClaimsPrincipal(identity));

            _log.Info($"Login success: {username}");

            return RedirectToAction("Index", "Home");
        }

        _log.Error($"Login failed: {username}");

        ViewBag.Error = "loginfailure";
        return View();
    }

    // GET: /logout
    [HttpGet("/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("cookie");

        _session.Clear();
        HttpContext.Session.Clear();

        return Redirect("/default");
    }
}