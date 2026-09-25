using backend.Data;
using backend.Models;
using backend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Implementation
{
    public class VideoRepository : IVideoRepository
    {
        private readonly CrochetFileShelfDbContext _context;

        public VideoRepository(CrochetFileShelfDbContext context)
        {
            _context = context;
        }

        public Task<VideoReference?> GetByWorkIdAsync(Guid workId) =>
            _context.VideoReferences
                .Include(v => v.Work)
                .FirstOrDefaultAsync(v => v.WorkId == workId);

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
