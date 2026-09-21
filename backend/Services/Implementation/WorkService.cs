using backend.Dtos.Works;
using backend.Exceptions;
using backend.Models;
using backend.Repository.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementation
{
    public class WorkService : IWorkService
    {
        private readonly IWorkRepository _workRepository;
        private readonly IFolderRepository _folderRepository;

        public WorkService(IWorkRepository workRepository, IFolderRepository folderRepository)
        {
            _workRepository = workRepository;
            _folderRepository = folderRepository;
        }

        public async Task<List<WorkResponse>> GetAsync(Guid userId, Guid? folderId)
        {
            var works = await _workRepository.GetByOwnerAsync(userId, folderId);
            return works.Select(ToResponse).ToList();
        }

        public async Task<WorkResponse> GetByIdAsync(Guid userId, Guid workId)
        {
            var work = await GetOwnedWorkAsync(userId, workId);
            return ToResponse(work);
        }

        public async Task<WorkResponse> CreateAsync(Guid userId, CreateWorkRequest request)
        {
            await EnsureFolderOwnedAsync(userId, request.FolderId!.Value);

            var now = DateTime.UtcNow;
            var work = new Work
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Description = NormalizeDescription(request.Description),
                Type = request.Type!.Value,
                OwnerId = userId,
                FolderId = request.FolderId.Value,
                CreatedAt = now,
                UpdatedAt = now,
            };

            await _workRepository.AddAsync(work);
            await _workRepository.SaveChangesAsync();

            return ToResponse(work);
        }

        public async Task<WorkResponse> UpdateAsync(Guid userId, Guid workId, UpdateWorkRequest request)
        {
            var work = await GetOwnedWorkAsync(userId, workId);

            work.Name = request.Name.Trim();
            work.Description = NormalizeDescription(request.Description);
            work.UpdatedAt = DateTime.UtcNow;
            await _workRepository.SaveChangesAsync();

            return ToResponse(work);
        }

        public async Task<WorkResponse> MoveAsync(Guid userId, Guid workId, MoveWorkRequest request)
        {
            var work = await GetOwnedWorkAsync(userId, workId);
            await EnsureFolderOwnedAsync(userId, request.FolderId!.Value);

            work.FolderId = request.FolderId.Value;
            work.UpdatedAt = DateTime.UtcNow;
            await _workRepository.SaveChangesAsync();

            return ToResponse(work);
        }

        public async Task DeleteAsync(Guid userId, Guid workId)
        {
            var work = await GetOwnedWorkAsync(userId, workId);
            _workRepository.Remove(work);
            await _workRepository.SaveChangesAsync();
        }

        private async Task<Work> GetOwnedWorkAsync(Guid userId, Guid workId)
        {
            var work = await _workRepository.GetByIdAsync(workId, userId);
            return work ?? throw new NotFoundException("Work not found.");
        }

        private async Task EnsureFolderOwnedAsync(Guid userId, Guid folderId)
        {
            var folder = await _folderRepository.GetByIdAsync(folderId, userId);
            if (folder is null)
            {
                throw new NotFoundException("Folder not found.");
            }
        }

        private static string? NormalizeDescription(string? description)
        {
            var trimmed = description?.Trim();
            return string.IsNullOrEmpty(trimmed) ? null : trimmed;
        }

        private static WorkResponse ToResponse(Work work) => new()
        {
            Id = work.Id,
            Name = work.Name,
            Description = work.Description,
            Type = work.Type,
            FolderId = work.FolderId,
            CreatedAt = work.CreatedAt,
            UpdatedAt = work.UpdatedAt,
        };
    }
}
