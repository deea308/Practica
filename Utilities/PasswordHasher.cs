
using System;
using System.Security.Cryptography;

namespace OnlineBookstore.Utilities
{
    // Secure password hashing and verification using PBKDF2 (SHA256)
    public static class PasswordHasher
    {
        private const byte Version = 0x01;
        private const int SaltSize = 16;     
        private const int HashSize = 32;     
        private const int Iterations = 100_000;

        public static string Hash(string password)
        {
            if (password is null) throw new ArgumentNullException(nameof(password));

            Span<byte> salt = stackalloc byte[SaltSize];
            RandomNumberGenerator.Fill(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt.ToArray(), Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(HashSize);

            var payload = new byte[1 + SaltSize + HashSize];
            payload[0] = Version;
            Buffer.BlockCopy(salt.ToArray(), 0, payload, 1, SaltSize);
            Buffer.BlockCopy(hash, 0, payload, 1 + SaltSize, HashSize);

            return Convert.ToBase64String(payload);
        }

        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(stored))
                return false;

            byte[] payload;
            try
            {
                payload = Convert.FromBase64String(stored);
            }
            catch
            {
                return false; 
            }

            if (payload.Length != 1 + SaltSize + HashSize || payload[0] != Version)
                return false;

            var salt = new byte[SaltSize];
            var expected = new byte[HashSize];
            Buffer.BlockCopy(payload, 1, salt, 0, SaltSize);
            Buffer.BlockCopy(payload, 1 + SaltSize, expected, 0, HashSize);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var actual = pbkdf2.GetBytes(HashSize);

            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }
    }
}
