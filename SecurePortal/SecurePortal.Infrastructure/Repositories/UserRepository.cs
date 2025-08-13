using SecurePortal.Infrastructure.Db;
using System.Data;

namespace SecurePortal.Infrastructure.Repositories;

public sealed class UserRepository
{
    private readonly ISimpleDb _db;
    public UserRepository(ISimpleDb db) => _db = db;

    public async Task InsertAsync(string id, string email, string pwdHash, string locale)
    {
        await _db.ExecuteAsync(
            "INSERT INTO users (id,email,pwd_hash,locale,created_utc) VALUES (@id,@em,@hp,@loc, now())",
            new Dictionary<string, object?> { ["id"]=id, ["em"]=email, ["hp"]=pwdHash, ["loc"]=locale }
        );
    }

    public async Task<int> UpdatePwdAsync(string id, string pwdHash)
        => await _db.ExecuteAsync("UPDATE users SET pwd_hash=@hp WHERE id=@id",
           new Dictionary<string, object?> { ["id"]=id, ["hp"]=pwdHash });

    public async Task<(string Id,string PwdHash,string Locale)?> FindByEmailAsync(string email)
    {
        return await _db.QuerySingleAsync(
            "SELECT id,pwd_hash,locale FROM users WHERE email=@em",
            new Dictionary<string, object?> { ["em"]=email },
            rd => (rd.GetString(0), rd.GetString(1), rd.GetString(2))
        );
    }
}
