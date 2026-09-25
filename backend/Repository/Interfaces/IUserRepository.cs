using backend.Models;

namespace backend.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId);
        Task AddAsync(User user);
        Task SaveChangesAsync();
    }
}
