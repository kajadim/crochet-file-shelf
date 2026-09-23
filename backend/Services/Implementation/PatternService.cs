using backend.Dtos.Patterns;
using backend.Exceptions;
using backend.Models;
using backend.Repository.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementation
{
    public class PatternService : IPatternService
    {
        private readonly IPatternRepository _patternRepository;
        private readonly IWorkRepository _workRepository;
        private readonly IYarnColorRepository _yarnColorRepository;

        public PatternService(
            IPatternRepository patternRepository,
            IWorkRepository workRepository,
            IYarnColorRepository yarnColorRepository)
        {
            _patternRepository = patternRepository;
            _workRepository = workRepository;
            _yarnColorRepository = yarnColorRepository;
        }

        public async Task<PatternResponse> GetAsync(Guid userId, Guid workId)
        {
            var pattern = await GetOwnedPatternAsync(userId, workId);
            var cells = await _patternRepository.GetAllCellsAsync(pattern.Id);
            return ToResponse(pattern, cells);
        }

        public async Task<PatternResponse> CreateAsync(Guid userId, Guid workId, CreatePatternRequest request)
        {
            var work = await _workRepository.GetByIdAsync(workId, userId);
            if (work is null)
            {
                throw new NotFoundException(ErrorCode.WorkNotFound);
            }
            if (work.Type != WorkType.Pattern)
            {
                throw new ConflictException(ErrorCode.WorkNotPatternType);
            }

            var existing = await _patternRepository.GetByWorkIdAsync(workId, userId);
            if (existing is not null)
            {
                throw new ConflictException(ErrorCode.PatternAlreadyExists);
            }

            var pattern = new Pattern
            {
                Id = Guid.NewGuid(),
                WorkId = workId,
                Width = request.Width!.Value,
                Height = request.Height!.Value,
                CurrentRow = 0,
                CurrentColumn = 0,
            };

            await _patternRepository.AddAsync(pattern);
            await _patternRepository.SaveChangesAsync();

            return ToResponse(pattern, []);
        }

        public async Task<PatternResponse> UpdatePositionAsync(Guid userId, Guid workId, UpdatePositionRequest request)
        {
            var pattern = await GetOwnedPatternAsync(userId, workId);

            var row = request.Row!.Value;
            var column = request.Column!.Value;
            if (row >= pattern.Height || column >= pattern.Width)
            {
                throw new BadRequestException(ErrorCode.PatternOutOfBounds);
            }

            pattern.CurrentRow = row;
            pattern.CurrentColumn = column;
            await _patternRepository.SaveChangesAsync();

            var cells = await _patternRepository.GetAllCellsAsync(pattern.Id);
            return ToResponse(pattern, cells);
        }

        public async Task<PatternResponse> UpdateActiveRowAsync(Guid userId, Guid workId, UpdateActiveRowRequest request)
        {
            var pattern = await GetOwnedPatternAsync(userId, workId);

            if (request.Row.HasValue && request.Row.Value >= pattern.Height)
            {
                throw new BadRequestException(ErrorCode.PatternOutOfBounds);
            }

            pattern.ActiveRow = request.Row;
            await _patternRepository.SaveChangesAsync();

            var cells = await _patternRepository.GetAllCellsAsync(pattern.Id);
            return ToResponse(pattern, cells);
        }

        public async Task<PatternResponse> ExpandAsync(Guid userId, Guid workId, PatternEdgesRequest request)
        {
            var pattern = await GetOwnedPatternAsync(userId, workId);

            var newWidth = pattern.Width + request.Left + request.Right;
            var newHeight = pattern.Height + request.Top + request.Bottom;
            if (newWidth > 200 || newHeight > 200)
            {
                throw new BadRequestException(ErrorCode.PatternTooLarge);
            }

            var cells = await _patternRepository.GetAllCellsAsync(pattern.Id);

            pattern.Width = newWidth;
            pattern.Height = newHeight;
            pattern.CurrentRow += request.Top;
            pattern.CurrentColumn += request.Left;
            if (pattern.ActiveRow.HasValue)
            {
                pattern.ActiveRow += request.Top;
            }

            await _patternRepository.ApplyExpansionAsync(cells, request.Top, request.Left);

            return ToResponse(pattern, cells);
        }

        public async Task<PatternResponse> ShrinkAsync(Guid userId, Guid workId, PatternEdgesRequest request)
        {
            var pattern = await GetOwnedPatternAsync(userId, workId);

            var newWidth = pattern.Width - request.Left - request.Right;
            var newHeight = pattern.Height - request.Top - request.Bottom;
            if (newWidth < 1 || newHeight < 1)
            {
                throw new BadRequestException(ErrorCode.PatternTooSmall);
            }

            var cells = await _patternRepository.GetAllCellsAsync(pattern.Id);
            var remaining = new List<PatternCell>();
            foreach (var cell in cells)
            {
                var inside = cell.RowIndex >= request.Top && cell.RowIndex < request.Top + newHeight
                    && cell.ColumnIndex >= request.Left && cell.ColumnIndex < request.Left + newWidth;
                if (inside)
                {
                    remaining.Add(cell);
                }
                else
                {
                    _patternRepository.RemoveCell(cell);
                }
            }

            pattern.Width = newWidth;
            pattern.Height = newHeight;
            pattern.CurrentRow = Math.Clamp(pattern.CurrentRow - request.Top, 0, newHeight - 1);
            pattern.CurrentColumn = Math.Clamp(pattern.CurrentColumn - request.Left, 0, newWidth - 1);
            if (pattern.ActiveRow.HasValue)
            {
                var activeRow = pattern.ActiveRow.Value - request.Top;
                pattern.ActiveRow = activeRow >= 0 && activeRow < newHeight ? activeRow : null;
            }

            await _patternRepository.ApplyExpansionAsync(remaining, -request.Top, -request.Left);

            return ToResponse(pattern, remaining);
        }

        public async Task<PatternResponse> SetCellsAsync(Guid userId, Guid workId, SetCellsRequest request)
        {
            var pattern = await GetOwnedPatternAsync(userId, workId);

            var changes = new Dictionary<(int Row, int Column), CellChange>();
            foreach (var change in request.Cells)
            {
                changes[(change.Row, change.Column)] = change;
            }

            foreach (var change in changes.Values)
            {
                if (change.Row >= pattern.Height || change.Column >= pattern.Width)
                {
                    throw new BadRequestException(ErrorCode.PatternOutOfBounds);
                }
            }

            var colorIds = changes.Values.Where(c => c.ColorId.HasValue).Select(c => c.ColorId!.Value).Distinct();
            foreach (var colorId in colorIds)
            {
                var color = await _yarnColorRepository.GetActiveByIdAsync(colorId, userId);
                if (color is null)
                {
                    throw new NotFoundException(ErrorCode.ColorNotFound);
                }
            }

            var existingCells = await _patternRepository.GetAllCellsAsync(pattern.Id);
            var existingByPosition = existingCells.ToDictionary(c => (c.RowIndex, c.ColumnIndex));

            foreach (var change in changes.Values)
            {
                existingByPosition.TryGetValue((change.Row, change.Column), out var existingCell);

                if (change.ColorId is null)
                {
                    if (existingCell is not null)
                    {
                        _patternRepository.RemoveCell(existingCell);
                    }
                    continue;
                }

                if (existingCell is not null)
                {
                    existingCell.YarnColorId = change.ColorId.Value;
                }
                else
                {
                    await _patternRepository.AddCellAsync(new PatternCell
                    {
                        Id = Guid.NewGuid(),
                        PatternId = pattern.Id,
                        RowIndex = change.Row,
                        ColumnIndex = change.Column,
                        YarnColorId = change.ColorId.Value,
                    });
                }
            }

            await _patternRepository.SaveChangesAsync();

            var updatedCells = await _patternRepository.GetAllCellsAsync(pattern.Id);
            return ToResponse(pattern, updatedCells);
        }

        private async Task<Pattern> GetOwnedPatternAsync(Guid userId, Guid workId)
        {
            var pattern = await _patternRepository.GetByWorkIdAsync(workId, userId);
            return pattern ?? throw new NotFoundException(ErrorCode.PatternNotFound);
        }

        private static PatternResponse ToResponse(Pattern pattern, List<PatternCell> cells) => new()
        {
            Width = pattern.Width,
            Height = pattern.Height,
            CurrentRow = pattern.CurrentRow,
            CurrentColumn = pattern.CurrentColumn,
            ActiveRow = pattern.ActiveRow,
            Cells = cells.Select(c => new PatternCellResponse
            {
                Row = c.RowIndex,
                Column = c.ColumnIndex,
                ColorId = c.YarnColorId,
                HexValue = c.YarnColor.HexValue,
            }).ToList(),
        };
    }
}
