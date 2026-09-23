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

        public Task<SiteReference?> GetByWorkIdAsync(Guid workId, Guid ownerId) =>
            _context.SiteReferences
                .Include(s => s.Work)
                .FirstOrDefaultAsync(s => s.WorkId == workId && s.Work.OwnerId == ownerId);

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
