using backend.Data;
using backend.Models;
using backend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly CrochetFileShelfDbContext _context;

        public UserRepository(CrochetFileShelfDbContext context)
        {
            _context = context;
        }

        public Task<User?> GetByEmailAsync(string email) =>
            _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public Task<User?> GetByIdAsync(Guid id) =>
            _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        public Task<bool> EmailExistsAsync(string email) =>
            _context.Users.AnyAsync(u => u.Email == email);

        public async Task AddAsync(User user) =>
            await _context.Users.AddAsync(user);

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
