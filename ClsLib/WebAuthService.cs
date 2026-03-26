using ClsLib;

namespace Web.Services;

public class WebAuthService
{
    public bool Login(string username, string password)
    {
        var user = new ClsAdminUser
        {
            Username = username,
            Password = password
        };

        return user.Auth();
    }
}