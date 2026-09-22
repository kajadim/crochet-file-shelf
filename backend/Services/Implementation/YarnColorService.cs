using backend.Dtos.YarnColors;
using backend.Exceptions;
using backend.Models;
using backend.Repository.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementation
{
    public class YarnColorService : IYarnColorService
    {
        private readonly IYarnColorRepository _yarnColorRepository;

        public YarnColorService(IYarnColorRepository yarnColorRepository)
        {
            _yarnColorRepository = yarnColorRepository;
        }

        public async Task<List<YarnColorResponse>> GetAllAsync(Guid userId)
        {
            var colors = await _yarnColorRepository.GetActiveByOwnerAsync(userId);
            var usageCounts = await _yarnColorRepository.GetWorksUsingCountsAsync(userId);

            return colors
                .Select(color => ToResponse(color, usageCounts.GetValueOrDefault(color.Id)))
                .ToList();
        }

        public async Task<YarnColorResponse> CreateAsync(Guid userId, YarnColorRequest request)
        {
            var name = request.Name.Trim();
            await EnsureNameIsUniqueAsync(userId, name, null);

            var color = new YarnColor
            {
                Id = Guid.NewGuid(),
                Name = name,
                HexValue = request.HexValue.ToUpperInvariant(),
                Notes = NormalizeNotes(request.Notes),
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow,
            };

            await _yarnColorRepository.AddAsync(color);
            await _yarnColorRepository.SaveChangesAsync();

            return ToResponse(color, 0);
        }

        public async Task<YarnColorResponse> UpdateAsync(Guid userId, Guid colorId, YarnColorRequest request)
        {
            var color = await GetOwnedColorAsync(userId, colorId);
            var name = request.Name.Trim();
            await EnsureNameIsUniqueAsync(userId, name, color.Id);

            color.Name = name;
            color.HexValue = request.HexValue.ToUpperInvariant();
            color.Notes = NormalizeNotes(request.Notes);
            await _yarnColorRepository.SaveChangesAsync();

            var usageCount = await _yarnColorRepository.GetWorksUsingCountAsync(color.Id);
            return ToResponse(color, usageCount);
        }

        public async Task DeleteAsync(Guid userId, Guid colorId)
        {
            var color = await GetOwnedColorAsync(userId, colorId);
            var usageCount = await _yarnColorRepository.GetWorksUsingCountAsync(color.Id);

            if (usageCount > 0)
            {
                color.IsArchived = true;
            }
            else
            {
                _yarnColorRepository.Remove(color);
            }

            await _yarnColorRepository.SaveChangesAsync();
        }

        private async Task<YarnColor> GetOwnedColorAsync(Guid userId, Guid colorId)
        {
            var color = await _yarnColorRepository.GetActiveByIdAsync(colorId, userId);
            return color ?? throw new NotFoundException(ErrorCode.ColorNotFound);
        }

        private async Task EnsureNameIsUniqueAsync(Guid userId, string name, Guid? excludeColorId)
        {
            if (await _yarnColorRepository.NameExistsAsync(userId, name, excludeColorId))
            {
                throw new ConflictException(ErrorCode.ColorNameConflict);
            }
        }

        private static string? NormalizeNotes(string? notes)
        {
            var trimmed = notes?.Trim();
            return string.IsNullOrEmpty(trimmed) ? null : trimmed;
        }

        private static YarnColorResponse ToResponse(YarnColor color, int worksUsingCount) => new()
        {
            Id = color.Id,
            Name = color.Name,
            HexValue = color.HexValue,
            Notes = color.Notes,
            CreatedAt = color.CreatedAt,
            WorksUsingCount = worksUsingCount,
        };
    }
}
