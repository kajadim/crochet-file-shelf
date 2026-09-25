namespace backend.Dtos.Auth
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = null!;
        public DateTime AccessTokenExpiresAt { get; set; }
        public UserSummary User { get; set; } = null!;
    }

    public class UserSummary
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public long? AvatarVersion { get; set; }
    }
}
