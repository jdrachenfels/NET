using ClsLib;

namespace Web.Services;

public class WebAuthService
{
    public ClsAdminUser? Authenticate(string username, string password)
    {
        var user = new ClsAdminUser
        {
            Username = username,
            Password = password
        };

        if (user.Auth())
            return user;

        return null;
    }
}