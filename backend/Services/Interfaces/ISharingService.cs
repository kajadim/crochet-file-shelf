using backend.Dtos.Sharing;
using backend.Models;

namespace backend.Services.Interfaces
{
    public interface ISharingService
    {
        Task<SharingResponse> GetAsync(Guid userId, Guid workId);
        Task<InvitationResponse> SetInvitationAsync(Guid userId, Guid workId, SetInvitationRequest request);
        Task RevokeInvitationAsync(Guid userId, Guid workId);
        Task<MemberResponse> UpdateMemberAsync(Guid userId, Guid workId, Guid memberUserId, UpdateMemberRequest request);
        Task RemoveMemberAsync(Guid userId, Guid workId, Guid memberUserId);
        Task LeaveAsync(Guid userId, Guid workId);
        Task<JoinWorkResponse> JoinAsync(Guid userId, JoinWorkRequest request);
        Task<bool> PrepareRemovalAsync(Work work);
    }
}
