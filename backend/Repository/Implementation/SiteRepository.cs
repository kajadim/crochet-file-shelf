using backend.Data;
using backend.Models;
using backend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Implementation
{
    public class SiteRepository : ISiteRepository
    {
        private readonly CrochetFileShelfDbContext _context;

        public SiteRepository(CrochetFileShelfDbContext context)
        {
            _context = context;
        }

        public Task<SiteReference?> GetByWorkIdAsync(Guid workId) =>
            _context.SiteReferences
                .Include(s => s.Work)
                .FirstOrDefaultAsync(s => s.WorkId == workId);

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
