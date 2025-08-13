using SecurePortal.Infrastructure.Db;

namespace SecurePortal.Infrastructure.Repositories;

public sealed class ReplyRepository
{
    private readonly ISimpleDb _db;
    public ReplyRepository(ISimpleDb db) => _db = db;

    public Task InsertAsync(string id, string threadId, string userId, string body)
        => _db.ExecuteAsync("INSERT INTO replies (id,message_id,user_id,body,created_utc) VALUES (@id,@mid,@uid,@b, now())",
            new Dictionary<string, object?> { ["id"]=id, ["mid"]=threadId, ["uid"]=userId, ["b"]=body });
}
