namespace backend.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public ICollection<Folder> Folders { get; set; } = new List<Folder>();
        public ICollection<Work> OwnedWorks { get; set; } = new List<Work>();
        public ICollection<YarnColor> YarnColors { get; set; } = new List<YarnColor>();
    }
}
