-- PostgreSQL 8.4 compatible schema
CREATE TABLE IF NOT EXISTS users (
  id           TEXT PRIMARY KEY,
  email        TEXT UNIQUE NOT NULL,
  pwd_hash     TEXT NOT NULL,
  locale       TEXT NOT NULL DEFAULT 'de',
  created_utc  TIMESTAMPTZ NOT NULL DEFAULT now(),
  last_login_utc TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS login_invites (
  id           TEXT PRIMARY KEY,
  email        TEXT NOT NULL,
  message_id   TEXT,
  expires_utc  TIMESTAMPTZ NOT NULL,
  used_utc     TIMESTAMPTZ
);
CREATE INDEX IF NOT EXISTS ix_login_invites_email ON login_invites(email);

CREATE TABLE IF NOT EXISTS threads (
  id           TEXT PRIMARY KEY,
  subject      TEXT,
  owner_email  TEXT NOT NULL,
  created_utc  TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS replies (
  id           TEXT PRIMARY KEY,
  message_id   TEXT NOT NULL REFERENCES threads(id),
  user_id      TEXT NOT NULL REFERENCES users(id),
  body         TEXT NOT NULL,
  created_utc  TIMESTAMPTZ NOT NULL DEFAULT now()
);
