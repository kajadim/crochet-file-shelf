using backend.Dtos.Folders;
using backend.Exceptions;
using backend.Models;
using backend.Repository.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementation
{
    public class FolderService : IFolderService
    {
        private readonly IFolderRepository _folderRepository;

        public FolderService(IFolderRepository folderRepository)
        {
            _folderRepository = folderRepository;
        }

        public async Task<List<FolderResponse>> GetAllAsync(Guid userId)
        {
            var folders = await _folderRepository.GetAllByOwnerAsync(userId);
            return folders.Select(ToResponse).ToList();
        }

        public async Task<FolderResponse> CreateAsync(Guid userId, CreateFolderRequest request)
        {
            if (request.ParentFolderId.HasValue)
            {
                var parent = await _folderRepository.GetByIdAsync(request.ParentFolderId.Value, userId);
                if (parent is null)
                {
                    throw new NotFoundException("Parent folder not found.");
                }
            }

            var name = request.Name.Trim();
            await EnsureNameIsUniqueAsync(userId, request.ParentFolderId, name, null);

            var folder = new Folder
            {
                Id = Guid.NewGuid(),
                Name = name,
                OwnerId = userId,
                ParentFolderId = request.ParentFolderId,
                CreatedAt = DateTime.UtcNow,
            };

            await _folderRepository.AddAsync(folder);
            await _folderRepository.SaveChangesAsync();

            return ToResponse(folder);
        }

        public async Task<FolderResponse> RenameAsync(Guid userId, Guid folderId, RenameFolderRequest request)
        {
            var folder = await GetOwnedFolderAsync(userId, folderId);
            var name = request.Name.Trim();
            await EnsureNameIsUniqueAsync(userId, folder.ParentFolderId, name, folder.Id);

            folder.Name = name;
            await _folderRepository.SaveChangesAsync();

            return ToResponse(folder);
        }

        public async Task<FolderDeletionSummary> GetDeletionSummaryAsync(Guid userId, Guid folderId)
        {
            await GetOwnedFolderAsync(userId, folderId);

            var descendantIds = await GetFolderAndDescendantIdsAsync(userId, folderId);
            var workCount = await _folderRepository.CountWorksInFoldersAsync(descendantIds);

            return new FolderDeletionSummary
            {
                SubfolderCount = descendantIds.Count - 1,
                WorkCount = workCount,
            };
        }

        public async Task DeleteAsync(Guid userId, Guid folderId)
        {
            var folder = await GetOwnedFolderAsync(userId, folderId);
            _folderRepository.Remove(folder);
            await _folderRepository.SaveChangesAsync();
        }

        private async Task EnsureNameIsUniqueAsync(Guid userId, Guid? parentFolderId, string name, Guid? excludeFolderId)
        {
            if (await _folderRepository.NameExistsAmongSiblingsAsync(userId, parentFolderId, name, excludeFolderId))
            {
                throw new ConflictException("A folder with this name already exists here.");
            }
        }

        private async Task<Folder> GetOwnedFolderAsync(Guid userId, Guid folderId)
        {
            var folder = await _folderRepository.GetByIdAsync(folderId, userId);
            return folder ?? throw new NotFoundException("Folder not found.");
        }

        private async Task<List<Guid>> GetFolderAndDescendantIdsAsync(Guid userId, Guid folderId)
        {
            var allFolders = await _folderRepository.GetAllByOwnerAsync(userId);
            var childrenByParent = allFolders
                .Where(f => f.ParentFolderId.HasValue)
                .ToLookup(f => f.ParentFolderId!.Value, f => f.Id);

            var result = new List<Guid>();
            var queue = new Queue<Guid>();
            queue.Enqueue(folderId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                result.Add(current);

                foreach (var childId in childrenByParent[current])
                {
                    queue.Enqueue(childId);
                }
            }

            return result;
        }

        private static FolderResponse ToResponse(Folder folder) => new()
        {
            Id = folder.Id,
            Name = folder.Name,
            ParentFolderId = folder.ParentFolderId,
            CreatedAt = folder.CreatedAt,
        };
    }
}
