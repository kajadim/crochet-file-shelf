using backend.Models;

namespace backend.Repository.Interfaces
{
    public interface IPendingRegistrationRepository
    {
        Task<PendingRegistration?> GetByEmailAsync(string email);
        Task<bool> UsernameReservedAsync(string username, string exceptEmail);
        Task AddAsync(PendingRegistration pendingRegistration);
        void Remove(PendingRegistration pendingRegistration);
        Task SaveChangesAsync();
    }
}
