namespace Web.Models;

public class NavMenuItem
{
    public string Key { get; set; } = "";
    public string TextKey { get; set; } = "";
    public string Url { get; set; } = "";
    public string? Icon { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsActive { get; set; }
    public List<NavMenuItem> Children { get; set; } = new();
}