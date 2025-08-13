using System.Security.Cryptography;

namespace SecurePortal.Application.Services;

public static class PasswordHasher
{
    // Parameter ggf. in INI konfigurierbar machen
    private const int SaltSize = 16;         // 128 Bit
    private const int KeySize = 32;         // 256 Bit
    private const int Iter = 200_000;    // Work-Faktor

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iter,
            HashAlgorithmName.SHA256,
            KeySize);

        return $"$pbkdf2-sha256$i={Iter}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string stored)
    {
        // Format: $pbkdf2-sha256$i=<iter>$<salt_b64>$<hash_b64>
        var parts = stored.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 4 || !parts[0].Equals("pbkdf2-sha256", StringComparison.Ordinal))
            return false;

        var iterStr = parts[1].StartsWith("i=") ? parts[1].Substring(2) : "0";
        if (!int.TryParse(iterStr, out var iter) || iter <= 0) return false;

        var salt = Convert.FromBase64String(parts[2]);
        var expected = Convert.FromBase64String(parts[3]);

        var actual = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, iter, HashAlgorithmName.SHA256, expected.Length);

        return FixedTimeEquals(actual, expected);
    }

    private static bool FixedTimeEquals(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
    {
        if (a.Length != b.Length) return false;
        var diff = 0;
        for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
        return diff == 0;
    }
}
