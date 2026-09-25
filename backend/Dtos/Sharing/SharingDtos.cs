using System.ComponentModel.DataAnnotations;
using backend.Models;

namespace backend.Dtos.Sharing
{
    public class InvitationResponse
    {
        public string Code { get; set; } = null!;
        public WorkPermission Permission { get; set; }
        public bool IsActive { get; set; }
    }

    public class MemberResponse
    {
        public Guid UserId { get; set; }
        public string DisplayName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public long? AvatarVersion { get; set; }
        public WorkPermission Permission { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class SharingResponse
    {
        public InvitationResponse? Invitation { get; set; }
        public List<MemberResponse> Members { get; set; } = [];
    }

    public class SetInvitationRequest
    {
        [Required]
        [EnumDataType(typeof(WorkPermission))]
        public WorkPermission? Permission { get; set; }
    }

    public class UpdateMemberRequest
    {
        [Required]
        [EnumDataType(typeof(WorkPermission))]
        public WorkPermission? Permission { get; set; }
    }

    public class JoinWorkRequest
    {
        [Required]
        [MaxLength(32)]
        public string Code { get; set; } = null!;
    }

    public class JoinWorkResponse
    {
        public Guid WorkId { get; set; }
        public string Name { get; set; } = null!;
        public WorkType Type { get; set; }
        public WorkPermission Permission { get; set; }
    }
}
