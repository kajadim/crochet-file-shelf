using backend.Dtos.Videos;
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
        private readonly IVideoLinkService _videoLinkService;
        private readonly IWorkAccessService _access;
        private readonly ISharingService _sharingService;

        public WorkService(
            IWorkRepository workRepository,
            IFolderRepository folderRepository,
            IVideoLinkService videoLinkService,
            IWorkAccessService access,
            ISharingService sharingService)
        {
            _workRepository = workRepository;
            _folderRepository = folderRepository;
            _videoLinkService = videoLinkService;
            _access = access;
            _sharingService = sharingService;
        }

        public async Task<List<WorkResponse>> GetAsync(Guid userId, WorkQueryRequest query)
        {
            var filtersActive = !string.IsNullOrWhiteSpace(query.Search)
                || query.Type.HasValue
                || query.ColorId.HasValue
                || query.Platform.HasValue;
            var includeShared = filtersActive && !query.FolderId.HasValue;

            var works = await _workRepository.GetByOwnerAsync(
                userId, query.FolderId, query.Search, query.Type, query.ColorId, query.Platform, includeShared);

            return await ToResponsesAsync(userId, works);
        }

        public async Task<List<WorkResponse>> GetSharedAsync(Guid userId, WorkQueryRequest query)
        {
            var works = await _workRepository.GetSharedWithAsync(userId, query.Search, query.Type, query.ColorId, query.Platform);
            return await ToResponsesAsync(userId, works);
        }

        public async Task<WorkResponse> GetByIdAsync(Guid userId, Guid workId)
        {
            var access = await _access.RequireAsync(userId, workId, WorkAccessLevel.Read);
            return ToResponse(access.Work, access.Role, access.Role == WorkRole.Owner ? null : access.Work.Owner.DisplayName);
        }

        public async Task<WorkResponse> CreateAsync(Guid userId, CreateWorkRequest request)
        {
            await EnsureFolderOwnedAsync(userId, request.FolderId!.Value);

            VideoLinkInfo? link = null;
            if (request.Type == WorkType.Video)
            {
                if (string.IsNullOrWhiteSpace(request.Url))
                {
                    throw new BadRequestException(ErrorCode.VideoLinkRequired);
                }
                link = await _videoLinkService.ResolveAsync(request.Url);
            }

            string? siteUrl = null;
            if (request.Type == WorkType.Site)
            {
                if (string.IsNullOrWhiteSpace(request.Url))
                {
                    throw new BadRequestException(ErrorCode.SiteLinkInvalid);
                }
                siteUrl = SiteLinkNormalizer.Normalize(request.Url);
            }

            if (request.Type == WorkType.Pattern && request.Width.HasValue != request.Height.HasValue)
            {
                throw new BadRequestException(ErrorCode.PatternSizeIncomplete);
            }

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

            if (link is not null)
            {
                work.VideoReference = new VideoReference
                {
                    Id = Guid.NewGuid(),
                    Platform = link.Platform,
                    OriginalUrl = link.OriginalUrl,
                    NormalizedUrl = link.NormalizedUrl,
                    TimestampSeconds = link.TimestampSeconds,
                };
            }

            if (request.Type == WorkType.Pattern && request.Width.HasValue && request.Height.HasValue)
            {
                work.Pattern = new Pattern
                {
                    Id = Guid.NewGuid(),
                    Width = request.Width.Value,
                    Height = request.Height.Value,
                    CurrentRow = 0,
                    CurrentColumn = 0,
                };
            }

            if (siteUrl is not null)
            {
                work.SiteReference = new SiteReference { Id = Guid.NewGuid(), Url = siteUrl };
            }

            await _workRepository.AddAsync(work);
            await _workRepository.SaveChangesAsync();

            return ToResponse(work, WorkRole.Owner, null);
        }

        public async Task<WorkResponse> UpdateAsync(Guid userId, Guid workId, UpdateWorkRequest request)
        {
            var work = (await _access.RequireAsync(userId, workId, WorkAccessLevel.Owner)).Work;

            work.Name = request.Name.Trim();
            work.Description = NormalizeDescription(request.Description);
            work.UpdatedAt = DateTime.UtcNow;
            await _workRepository.SaveChangesAsync();

            return ToResponse(work, WorkRole.Owner, null);
        }

        public async Task<WorkResponse> MoveAsync(Guid userId, Guid workId, MoveWorkRequest request)
        {
            var work = (await _access.RequireAsync(userId, workId, WorkAccessLevel.Owner)).Work;
            await EnsureFolderOwnedAsync(userId, request.FolderId!.Value);

            work.FolderId = request.FolderId.Value;
            work.UpdatedAt = DateTime.UtcNow;
            await _workRepository.SaveChangesAsync();

            return ToResponse(work, WorkRole.Owner, null);
        }

        public async Task DeleteAsync(Guid userId, Guid workId)
        {
            var work = (await _access.RequireAsync(userId, workId, WorkAccessLevel.Owner)).Work;

            var transferred = await _sharingService.PrepareRemovalAsync(work);
            if (!transferred)
            {
                _workRepository.Remove(work);
            }

            await _workRepository.SaveChangesAsync();
        }

        private async Task<List<WorkResponse>> ToResponsesAsync(Guid userId, List<Work> works)
        {
            var foreignIds = works.Where(w => w.OwnerId != userId).Select(w => w.Id).ToList();
            var permissions = foreignIds.Count == 0
                ? new Dictionary<Guid, WorkPermission>()
                : await _workRepository.GetMemberPermissionsAsync(userId, foreignIds);

            return works.Select(work =>
            {
                if (work.OwnerId == userId)
                {
                    return ToResponse(work, WorkRole.Owner, null);
                }

                var role = permissions.GetValueOrDefault(work.Id) == WorkPermission.CanEdit
                    ? WorkRole.Editor
                    : WorkRole.Viewer;
                return ToResponse(work, role, work.Owner.DisplayName);
            }).ToList();
        }

        private async Task EnsureFolderOwnedAsync(Guid userId, Guid folderId)
        {
            var folder = await _folderRepository.GetByIdAsync(folderId, userId);
            if (folder is null)
            {
                throw new NotFoundException(ErrorCode.FolderNotFound);
            }
        }

        private static string? NormalizeDescription(string? description)
        {
            var trimmed = description?.Trim();
            return string.IsNullOrEmpty(trimmed) ? null : trimmed;
        }

        private static WorkResponse ToResponse(Work work, WorkRole role, string? ownerName) => new()
        {
            Id = work.Id,
            Name = work.Name,
            Description = work.Description,
            Type = work.Type,
            FolderId = work.FolderId,
            CreatedAt = work.CreatedAt,
            UpdatedAt = work.UpdatedAt,
            Role = role,
            OwnerName = ownerName,
        };
    }
}
