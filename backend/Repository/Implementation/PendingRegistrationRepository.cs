using backend.Data;
using backend.Models;
using backend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Implementation
{
    public class PendingRegistrationRepository : IPendingRegistrationRepository
    {
        private readonly CrochetFileShelfDbContext _context;

        public PendingRegistrationRepository(CrochetFileShelfDbContext context)
        {
            _context = context;
        }

        public Task<PendingRegistration?> GetByEmailAsync(string email) =>
            _context.PendingRegistrations.FirstOrDefaultAsync(p => p.Email == email);

        public Task<bool> UsernameReservedAsync(string username, string exceptEmail) =>
            _context.PendingRegistrations.AnyAsync(p =>
                p.Username == username && p.Email != exceptEmail && p.ExpiresAt > DateTime.UtcNow);

        public async Task AddAsync(PendingRegistration pendingRegistration) =>
            await _context.PendingRegistrations.AddAsync(pendingRegistration);

        public void Remove(PendingRegistration pendingRegistration) =>
            _context.PendingRegistrations.Remove(pendingRegistration);

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
