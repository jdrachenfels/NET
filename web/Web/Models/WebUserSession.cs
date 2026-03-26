namespace Web.Models;

public class WebUserSession
{
    public string Username { get; set; } = "";
    public string Lang { get; set; } = "en";
    public List<string> Permissions { get; set; } = new();
}