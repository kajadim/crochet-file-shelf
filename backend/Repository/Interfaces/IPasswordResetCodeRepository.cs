using backend.Models;

namespace backend.Repository.Interfaces
{
    public interface IPasswordResetCodeRepository
    {
        Task<PasswordResetCode?> GetActiveByUserIdAsync(Guid userId);
        Task AddAsync(PasswordResetCode resetCode);
        Task RemoveAllForUserAsync(Guid userId);
        Task SaveChangesAsync();
    }
}
