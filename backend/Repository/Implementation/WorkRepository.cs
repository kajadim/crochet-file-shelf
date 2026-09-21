using backend.Data;
using backend.Models;
using backend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Implementation
{
    public class WorkRepository : IWorkRepository
    {
        private readonly CrochetFileShelfDbContext _context;

        public WorkRepository(CrochetFileShelfDbContext context)
        {
            _context = context;
        }

        public Task<List<Work>> GetByOwnerAsync(Guid ownerId, Guid? folderId)
        {
            var query = _context.Works.Where(w => w.OwnerId == ownerId);

            if (folderId.HasValue)
            {
                query = query.Where(w => w.FolderId == folderId.Value);
            }

            return query.OrderBy(w => w.Name).ToListAsync();
        }

        public Task<Work?> GetByIdAsync(Guid id, Guid ownerId) =>
            _context.Works.FirstOrDefaultAsync(w => w.Id == id && w.OwnerId == ownerId);

        public async Task AddAsync(Work work) =>
            await _context.Works.AddAsync(work);

        public void Remove(Work work) =>
            _context.Works.Remove(work);

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
