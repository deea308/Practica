
using System;
using System.Security.Cryptography;
using System.Text;

namespace OnlineBookstore.Utilities
{
    // Provides compatibility checks against older password storage formats
    public static class LegacyPasswordMatcher
    {
        // Checks if a plain password matches a stored legacy hash
        public static bool Matches(string password, string stored)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(stored))
                return false;

            if (stored == password) return true;

            if (EqualsIgnoreCase(stored, Sha256Hex(password))) return true;

            if (stored == Sha256Base64(password)) return true;

            return false;
        }

        private static bool EqualsIgnoreCase(string a, string b)
            => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        private static string Sha256Hex(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private static string Sha256Base64(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }

        
    }
}
