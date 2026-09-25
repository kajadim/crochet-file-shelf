using backend.Models;

namespace backend.Repository.Interfaces
{
    public interface ISharingRepository
    {
        Task<WorkInvitation?> GetInvitationByWorkAsync(Guid workId);
        Task<WorkInvitation?> GetActiveInvitationByCodeAsync(string code);
        Task<bool> CodeExistsAsync(string code);
        Task AddInvitationAsync(WorkInvitation invitation);
        void RemoveInvitation(WorkInvitation invitation);
        Task<List<WorkMember>> GetMembersAsync(Guid workId);
        Task<WorkMember?> GetMemberAsync(Guid workId, Guid userId);
        Task AddMemberAsync(WorkMember member);
        void RemoveMember(WorkMember member);
        Task SaveChangesAsync();
    }
}
