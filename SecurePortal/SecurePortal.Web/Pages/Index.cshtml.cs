using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

public class IndexModel : PageModel
{
    private readonly IStringLocalizer<IndexModel> _L;
    public IndexModel(IStringLocalizer<IndexModel> L) => _L = L;
    public string Title => _L["Portal"];
    public string Message => _L["Willkommen!"];
}
