using SecurePortal.Infrastructure.Db;
using System.Data;

namespace SecurePortal.Infrastructure.Db;

public static class DbBootstrapper
{
    public static async Task EnsureSchemaAsync(ISimpleDb db)
    {
        await db.OpenAsync();

        // users
        if (!await TableExistsAsync(db, "public", "users"))
        {
            await db.ExecuteAsync(@"
CREATE TABLE users (
  id TEXT PRIMARY KEY,
  email TEXT UNIQUE NOT NULL,
  pwd_hash TEXT NOT NULL,
  locale TEXT NOT NULL DEFAULT 'de',
  created_utc TIMESTAMPTZ NOT NULL DEFAULT now(),
  last_login_utc TIMESTAMPTZ
)", new Dictionary<string, object?>());
        }

        // login_invites
        if (!await TableExistsAsync(db, "public", "login_invites"))
        {
            await db.ExecuteAsync(@"
CREATE TABLE login_invites (
  id TEXT PRIMARY KEY,
  email TEXT NOT NULL,
  message_id TEXT,
  expires_utc TIMESTAMPTZ NOT NULL,
  used_utc TIMESTAMPTZ
)", new Dictionary<string, object?>());

            await db.ExecuteAsync(
                "CREATE INDEX ix_login_invites_email ON login_invites(email)",
                new Dictionary<string, object?>());
        }

        // threads
        if (!await TableExistsAsync(db, "public", "threads"))
        {
            await db.ExecuteAsync(@"
CREATE TABLE threads (
  id TEXT PRIMARY KEY,
  subject TEXT,
  owner_email TEXT NOT NULL,
  created_utc TIMESTAMPTZ NOT NULL DEFAULT now()
)", new Dictionary<string, object?>());
        }

        // replies
        if (!await TableExistsAsync(db, "public", "replies"))
        {
            await db.ExecuteAsync(@"
CREATE TABLE replies (
  id TEXT PRIMARY KEY,
  message_id TEXT NOT NULL REFERENCES threads(id),
  user_id TEXT NOT NULL REFERENCES users(id),
  body TEXT NOT NULL,
  created_utc TIMESTAMPTZ NOT NULL DEFAULT now()
)", new Dictionary<string, object?>());
        }
    }

    // 8.4-kompatibler Existenzcheck (ohne to_regclass)
    private static async Task<bool> TableExistsAsync(ISimpleDb db, string schema, string table)
    {
        var row = await db.QuerySingleAsync(
            @"SELECT 1
              FROM pg_catalog.pg_class c
              JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace
             WHERE n.nspname = @s AND c.relname = @t AND c.relkind = 'r'",
            new Dictionary<string, object?> { ["@s"] = schema, ["@t"] = table },
            rd => 1
        );
        return row != 0;
    }
}
