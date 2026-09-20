namespace backend.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public string TokenHash { get; set; } = null!; // hash, ne sirovi token (kao lozinka)
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public Guid? ReplacedByTokenId { get; set; } // za rotaciju (Faza 0)

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
