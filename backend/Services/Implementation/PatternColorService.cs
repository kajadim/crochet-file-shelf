using backend.Dtos.Patterns;
using backend.Dtos.YarnColors;
using backend.Exceptions;
using backend.Models;
using backend.Repository.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementation
{
    public class PatternColorService : IPatternColorService
    {
        private readonly IPatternRepository _patternRepository;
        private readonly IYarnColorRepository _yarnColorRepository;
        private readonly IWorkAccessService _access;
        private readonly Dictionary<(Guid OwnerId, string Hex), YarnColor> _created = new();

        public PatternColorService(
            IPatternRepository patternRepository,
            IYarnColorRepository yarnColorRepository,
            IWorkAccessService access)
        {
            _patternRepository = patternRepository;
            _yarnColorRepository = yarnColorRepository;
            _access = access;
        }

        public async Task<List<PatternColorResponse>> GetForWorkAsync(Guid userId, Guid workId)
        {
            await _access.RequireAsync(userId, workId, WorkAccessLevel.Read);
            var pattern = await GetPatternAsync(workId);
            var colors = await _yarnColorRepository.GetUsedInPatternAsync(pattern.Id);

            return colors
                .Select(color => new PatternColorResponse
                {
                    Id = color.Id,
                    Name = color.Name,
                    HexValue = color.HexValue,
                    Notes = color.Notes,
                    IsMine = color.OwnerId == userId && !color.IsArchived,
                })
                .ToList();
        }

        public async Task<YarnColorResponse> CopyToPaletteAsync(Guid userId, Guid workId, Guid colorId)
        {
            await _access.RequireAsync(userId, workId, WorkAccessLevel.Read);
            var pattern = await GetPatternAsync(workId);

            var source = await _yarnColorRepository.GetByIdAsync(colorId);
            if (source is null || !await _yarnColorRepository.IsUsedInPatternAsync(colorId, pattern.Id))
            {
                throw new NotFoundException(ErrorCode.ColorNotFound);
            }

            var copy = await FindOrCreateForOwnerAsync(source, userId);
            await _yarnColorRepository.SaveChangesAsync();

            var usageCount = await _yarnColorRepository.GetWorksUsingCountAsync(copy.Id);
            return new YarnColorResponse
            {
                Id = copy.Id,
                Name = copy.Name,
                HexValue = copy.HexValue,
                Notes = copy.Notes,
                CreatedAt = copy.CreatedAt,
                WorksUsingCount = usageCount,
            };
        }

        public async Task MoveColorsToOwnerAsync(Guid workId, Guid ownerId, Guid? fromOwnerId = null)
        {
            var pattern = await _patternRepository.GetByWorkIdAsync(workId);
            if (pattern is null)
            {
                return;
            }

            var cells = await _patternRepository.GetAllCellsAsync(pattern.Id);
            foreach (var group in cells.Where(c => c.YarnColor.OwnerId != ownerId && (fromOwnerId == null || c.YarnColor.OwnerId == fromOwnerId)).GroupBy(c => c.YarnColorId))
            {
                var target = await FindOrCreateForOwnerAsync(group.First().YarnColor, ownerId);
                foreach (var cell in group)
                {
                    cell.YarnColor = target;
                }
            }
        }

        private async Task<YarnColor> FindOrCreateForOwnerAsync(YarnColor source, Guid ownerId)
        {
            if (source.OwnerId == ownerId && !source.IsArchived)
            {
                return source;
            }

            var hex = source.HexValue.ToUpperInvariant();
            if (_created.TryGetValue((ownerId, hex), out var pending))
            {
                return pending;
            }

            var existing = await _yarnColorRepository.FindActiveByHexAsync(ownerId, hex);
            if (existing is not null)
            {
                return existing;
            }

            var color = new YarnColor
            {
                Id = Guid.NewGuid(),
                Name = await UniqueNameAsync(ownerId, source.Name),
                HexValue = hex,
                Notes = source.Notes,
                OwnerId = ownerId,
                CreatedAt = DateTime.UtcNow,
            };

            await _yarnColorRepository.AddAsync(color);
            _created[(ownerId, hex)] = color;
            return color;
        }

        private async Task<string> UniqueNameAsync(Guid ownerId, string name)
        {
            var candidate = name;
            var suffix = 2;
            while (await _yarnColorRepository.NameExistsAsync(ownerId, candidate, null)
                || _created.Any(pair => pair.Key.OwnerId == ownerId
                    && string.Equals(pair.Value.Name, candidate, StringComparison.OrdinalIgnoreCase)))
            {
                candidate = $"{name} ({suffix})";
                suffix++;
            }

            return candidate;
        }

        private async Task<Pattern> GetPatternAsync(Guid workId)
        {
            var pattern = await _patternRepository.GetByWorkIdAsync(workId);
            return pattern ?? throw new NotFoundException(ErrorCode.PatternNotFound);
        }
    }
}
