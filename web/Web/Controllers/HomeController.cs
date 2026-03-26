using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    [HttpGet("/home")]
    public IActionResult Index()
    {
        return View();
    }
}