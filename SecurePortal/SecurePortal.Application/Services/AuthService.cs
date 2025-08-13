using SecurePortal.Application.Services;
using SecurePortal.Infrastructure.Db;
using SecurePortal.Infrastructure.Repositories;

namespace SecurePortal.Application.Services;

public sealed class AuthService
{
    private readonly ISimpleDb _db;
    private readonly UserRepository _users;
    private readonly InviteRepository _invites;

    public AuthService(ISimpleDb db)
    { _db = db; _users = new UserRepository(db); _invites = new InviteRepository(db); }

    public async Task CreateOrUpdateUserAsync(string email, string password, string locale)
    {
        await _db.OpenAsync();
        var id = Guid.NewGuid().ToString();
        var existing = await _users.FindByEmailAsync(email);
        if (existing is null)
            await _users.InsertAsync(id, email, PasswordHasher.Hash(password), locale);
        else
            await _users.UpdatePwdAsync(existing.Value.Id, PasswordHasher.Hash(password));
    }
}
