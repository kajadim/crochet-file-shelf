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
        private readonly IWorkAccessService _access;
        private readonly IYarnColorRepository _yarnColorRepository;
        private readonly IRealtimeOutbox _outbox;

        public PatternService(
            IPatternRepository patternRepository,
            IWorkAccessService access,
            IYarnColorRepository yarnColorRepository,
            IRealtimeOutbox outbox)
        {
            _patternRepository = patternRepository;
            _access = access;
            _yarnColorRepository = yarnColorRepository;
            _outbox = outbox;
        }

        public async Task<PatternResponse> GetAsync(Guid userId, Guid workId)
        {
            var pattern = await GetPatternAsync(userId, workId, WorkAccessLevel.Read);
            var cells = await _patternRepository.GetAllCellsAsync(pattern.Id);
            return ToResponse(pattern, cells);
        }

        public async Task<PatternResponse> CreateAsync(Guid userId, Guid workId, CreatePatternRequest request)
        {
            var work = (await _access.RequireAsync(userId, workId, WorkAccessLevel.Owner)).Work;
            if (work.Type != WorkType.Pattern)
            {
                throw new ConflictException(ErrorCode.WorkNotPatternType);
            }

            var existing = await _patternRepository.GetByWorkIdAsync(workId);
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
            var pattern = await GetPatternAsync(userId, workId, WorkAccessLevel.Edit);

            var row = request.Row!.Value;
            var column = request.Column!.Value;
            if (row >= pattern.Height || column >= pattern.Width)
            {
                throw new BadRequestException(ErrorCode.PatternOutOfBounds);
            }

            pattern.CurrentRow = row;
            pattern.CurrentColumn = column;
            await _patternRepository.SaveChangesAsync();
            _outbox.Enqueue(n => n.PositionChangedAsync(workId, row, column));

            var cells = await _patternRepository.GetAllCellsAsync(pattern.Id);
            return ToResponse(pattern, cells);
        }

        public async Task<PatternResponse> UpdateActiveRowAsync(Guid userId, Guid workId, UpdateActiveRowRequest request)
        {
            var pattern = await GetPatternAsync(userId, workId, WorkAccessLevel.Edit);

            if (request.Row.HasValue && request.Row.Value >= pattern.Height)
            {
                throw new BadRequestException(ErrorCode.PatternOutOfBounds);
            }

            pattern.ActiveRow = request.Row;
            await _patternRepository.SaveChangesAsync();
            _outbox.Enqueue(n => n.ActiveRowChangedAsync(workId, request.Row));

            var cells = await _patternRepository.GetAllCellsAsync(pattern.Id);
            return ToResponse(pattern, cells);
        }

        public async Task<PatternResponse> ExpandAsync(Guid userId, Guid workId, PatternEdgesRequest request)
        {
            var pattern = await GetPatternAsync(userId, workId, WorkAccessLevel.Edit);

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
            _outbox.Enqueue(n => n.PatternResetAsync(workId));

            return ToResponse(pattern, cells);
        }

        public async Task<PatternResponse> ShrinkAsync(Guid userId, Guid workId, PatternEdgesRequest request)
        {
            var pattern = await GetPatternAsync(userId, workId, WorkAccessLevel.Edit);

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
            _outbox.Enqueue(n => n.PatternResetAsync(workId));

            return ToResponse(pattern, remaining);
        }

        public async Task<PatternResponse> SetCellsAsync(Guid userId, Guid workId, SetCellsRequest request)
        {
            var pattern = await GetPatternAsync(userId, workId, WorkAccessLevel.Edit);

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
            var hexByColor = new Dictionary<Guid, string>();
            foreach (var colorId in colorIds)
            {
                var color = await _yarnColorRepository.GetActiveByIdAsync(colorId, userId);
                if (color is null && await _yarnColorRepository.IsUsedInPatternAsync(colorId, pattern.Id))
                {
                    color = await _yarnColorRepository.GetByIdAsync(colorId);
                }

                if (color is null)
                {
                    throw new NotFoundException(ErrorCode.ColorNotFound);
                }
                hexByColor[colorId] = color.HexValue;
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

            var events = changes.Values
                .Select(c => new CellEvent(c.Row, c.Column, c.ColorId, c.ColorId.HasValue ? hexByColor[c.ColorId.Value] : null))
                .ToList();
            _outbox.Enqueue(n => n.CellsChangedAsync(workId, events));

            var updatedCells = await _patternRepository.GetAllCellsAsync(pattern.Id);
            return ToResponse(pattern, updatedCells);
        }

        private async Task<Pattern> GetPatternAsync(Guid userId, Guid workId, WorkAccessLevel level)
        {
            await _access.RequireAsync(userId, workId, level);
            var pattern = await _patternRepository.GetByWorkIdAsync(workId);
            return pattern ?? throw new NotFoundException(ErrorCode.PatternNotFound);
        }

        internal static PatternResponse ToResponse(Pattern pattern, List<PatternCell> cells) => new()
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
