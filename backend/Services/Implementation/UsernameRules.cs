using System.Text.RegularExpressions;

namespace backend.Services.Implementation
{
    public static class UsernameRules
    {
        public const string Pattern = "^[a-z0-9._-]{3,30}$";

        private static readonly Regex Valid = new(Pattern, RegexOptions.Compiled);

        public static string Normalize(string username) => username.Trim().ToLowerInvariant();

        public static bool IsValid(string normalizedUsername) => Valid.IsMatch(normalizedUsername);
    }
}
