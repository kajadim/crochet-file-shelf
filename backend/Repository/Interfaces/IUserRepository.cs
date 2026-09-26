using backend.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace backend.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId);
        Task AddAsync(User user);
        void Remove(User user);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task SaveChangesAsync();
    }
}
