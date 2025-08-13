using SecurePortal.Infrastructure.Db;

namespace SecurePortal.Infrastructure.Repositories;

public sealed class InviteRepository
{
    private readonly ISimpleDb _db;
    public InviteRepository(ISimpleDb db) => _db = db;

    public Task InsertAsync(string id, string email, DateTime expiresUtc, string? messageId)
        => _db.ExecuteAsync(
            "INSERT INTO login_invites (id,email,expires_utc,message_id) VALUES (@id,@em,@ex,@mid)",
            new Dictionary<string, object?> { ["id"]=id, ["em"]=email, ["ex"]=expiresUtc, ["mid"]=messageId }
        );

    public Task<int> MarkUsedAsync(string id)
        => _db.ExecuteAsync("UPDATE login_invites SET used_utc=now() WHERE id=@id AND used_utc IS NULL",
            new Dictionary<string, object?> { ["id"]=id });
}
