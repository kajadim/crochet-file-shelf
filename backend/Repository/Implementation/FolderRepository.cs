using backend.Data;
using backend.Models;
using backend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Implementation
{
    public class FolderRepository : IFolderRepository
    {
        private readonly CrochetFileShelfDbContext _context;

        public FolderRepository(CrochetFileShelfDbContext context)
        {
            _context = context;
        }

        public Task<List<Folder>> GetAllByOwnerAsync(Guid ownerId) =>
            _context.Folders
                .Where(f => f.OwnerId == ownerId)
                .OrderBy(f => f.Name)
                .ToListAsync();

        public Task<Folder?> GetByIdAsync(Guid id, Guid ownerId) =>
            _context.Folders.FirstOrDefaultAsync(f => f.Id == id && f.OwnerId == ownerId);

        public Task<bool> NameExistsAmongSiblingsAsync(Guid ownerId, Guid? parentFolderId, string name, Guid? excludeFolderId)
        {
            var lowerName = name.ToLower();

            return _context.Folders.AnyAsync(f =>
                f.OwnerId == ownerId
                && f.ParentFolderId == parentFolderId
                && f.Name.ToLower() == lowerName
                && (excludeFolderId == null || f.Id != excludeFolderId));
        }

        public Task<int> CountWorksInFoldersAsync(IReadOnlyCollection<Guid> folderIds) =>
            _context.Works.CountAsync(w => folderIds.Contains(w.FolderId));

        public async Task AddAsync(Folder folder) =>
            await _context.Folders.AddAsync(folder);

        public void Remove(Folder folder) =>
            _context.Folders.Remove(folder);

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
