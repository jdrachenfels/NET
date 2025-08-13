using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecurePortal.Application.Services;
using SecurePortal.Infrastructure.Db;

public class SetPasswordModel : PageModel
{
    private readonly ISimpleDb _db;
    public SetPasswordModel(ISimpleDb db) => _db = db;

    [BindProperty] public string Email { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    [BindProperty] public string Repeat { get; set; } = string.Empty;

    public async Task<IActionResult> OnPostAsync()
    {
        if (Password != Repeat) return Page();
        var svc = new AuthService(_db);
        await svc.CreateOrUpdateUserAsync(Email, Password, "de");
        return RedirectToPage("/Login");
    }
}
