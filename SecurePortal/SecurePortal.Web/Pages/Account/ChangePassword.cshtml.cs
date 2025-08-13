using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class ChangePasswordModel : PageModel
{
    [BindProperty] public string OldPassword { get; set; } = string.Empty;
    [BindProperty] public string NewPassword { get; set; } = string.Empty;
    [BindProperty] public string Repeat { get; set; } = string.Empty;

    public IActionResult OnPost()
    {
        // TODO: verify old, update new via repo
        return RedirectToPage("/Index");
    }
}
