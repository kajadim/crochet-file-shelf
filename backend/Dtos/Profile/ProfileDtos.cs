using backend.Models;

namespace backend.Dtos.Profile
{
    public class WorkCounts
    {
        public int Pattern { get; set; }
        public int Video { get; set; }
        public int Site { get; set; }
    }

    public class ProfileSharedWork
    {
        public Guid WorkId { get; set; }
        public string Name { get; set; } = null!;
        public WorkType Type { get; set; }
        public WorkPermission Permission { get; set; }
    }

    public class ProfileCollaborator
    {
        public Guid UserId { get; set; }
        public string DisplayName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public long? AvatarVersion { get; set; }
        public List<ProfileSharedWork> Works { get; set; } = [];
    }

    public class ProfileResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string? Bio { get; set; }
        public long? AvatarVersion { get; set; }
        public DateTime CreatedAt { get; set; }
        public WorkCounts WorkCounts { get; set; } = new();
        public List<ProfileCollaborator> SharedByMe { get; set; } = [];
        public List<ProfileCollaborator> SharedWithMe { get; set; } = [];
    }

    public record AvatarFile(byte[] Bytes, long Version);
}
