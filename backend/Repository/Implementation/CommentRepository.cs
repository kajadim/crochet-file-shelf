using backend.Data;
using backend.Models;
using backend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Implementation
{
    public class CommentRepository : ICommentRepository
    {
        private readonly CrochetFileShelfDbContext _context;

        public CommentRepository(CrochetFileShelfDbContext context)
        {
            _context = context;
        }

        public Task<List<WorkComment>> GetByWorkAsync(Guid workId) =>
            _context.WorkComments
                .Include(c => c.Author)
                .Where(c => c.WorkId == workId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

        public Task<WorkComment?> GetByIdAsync(Guid commentId, Guid workId) =>
            _context.WorkComments
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.Id == commentId && c.WorkId == workId);

        public async Task AddAsync(WorkComment comment) =>
            await _context.WorkComments.AddAsync(comment);

        public void Remove(WorkComment comment) =>
            _context.WorkComments.Remove(comment);

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
