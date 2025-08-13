using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class LoginModel : PageModel
{
    [BindProperty] public string Email { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;

    public IActionResult OnPost()
    {
        // TODO: echte Prüfung via Repo + Cookie-Auth-SignIn
        return RedirectToPage("Index");
    }
}
