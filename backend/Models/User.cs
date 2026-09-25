namespace backend.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string? Bio { get; set; }
        public DateTime? AvatarUpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public string DisplayName => $"{FirstName} {LastName}".Trim();

        public long? AvatarVersion => AvatarUpdatedAt is null
            ? null
            : new DateTimeOffset(DateTime.SpecifyKind(AvatarUpdatedAt.Value, DateTimeKind.Utc)).ToUnixTimeMilliseconds();

        public ICollection<Folder> Folders { get; set; } = new List<Folder>();
        public ICollection<Work> OwnedWorks { get; set; } = new List<Work>();
        public ICollection<YarnColor> YarnColors { get; set; } = new List<YarnColor>();
    }
}
