using backend.Models;

namespace backend.Repository.Interfaces
{
    public interface IProfileRepository
    {
        Task<Dictionary<WorkType, int>> GetWorkCountsAsync(Guid userId);
        Task<List<WorkMember>> GetMembersOfOwnedWorksAsync(Guid ownerId);
        Task<List<WorkMember>> GetMembershipsAsync(Guid userId);
        Task<byte[]?> GetAvatarAsync(Guid userId);
        Task SetAvatarAsync(Guid userId, byte[] data);
        Task RemoveAvatarAsync(Guid userId);
        Task SaveChangesAsync();
    }
}
