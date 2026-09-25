using backend.Data;
using backend.Models;
using backend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Implementation
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly CrochetFileShelfDbContext _context;

        public NotificationRepository(CrochetFileShelfDbContext context)
        {
            _context = context;
        }

        public Task<List<Notification>> GetLatestAsync(Guid recipientId, int take) =>
            _context.Notifications
                .Where(n => n.RecipientId == recipientId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .ToListAsync();

        public Task<int> CountUnreadAsync(Guid recipientId) =>
            _context.Notifications.CountAsync(n => n.RecipientId == recipientId && !n.IsRead);

        public Task<Notification?> GetByIdAsync(Guid id, Guid recipientId) =>
            _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.RecipientId == recipientId);

        public Task<List<Notification>> GetUnreadAsync(Guid recipientId) =>
            _context.Notifications.Where(n => n.RecipientId == recipientId && !n.IsRead).ToListAsync();

        public async Task AddAsync(Notification notification) =>
            await _context.Notifications.AddAsync(notification);

        public void Remove(Notification notification) =>
            _context.Notifications.Remove(notification);

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
