using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Web.Services;

namespace Web.Controllers;

public class AccountController : Controller
{
    private readonly WebAuthService _auth;
    private readonly WebSyslogService _log;

    public AccountController(WebAuthService auth, WebSyslogService log)
    {
        _auth = auth;
        _log = log;
    }

    [HttpGet("/default")]
    public IActionResult Login() => View();

    [HttpPost("/default")]
    public async Task<IActionResult> Login(string username, string password, string lang)
    {
        HttpContext.Session.SetString("lang", lang ?? "en");

        if (_auth.Login(username, password))
        {
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

    [HttpGet("/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("cookie");
        HttpContext.Session.Clear();

        return Redirect("/default");
    }
}