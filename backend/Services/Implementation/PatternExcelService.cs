using System.Text.RegularExpressions;
using backend.Dtos.Patterns;
using backend.Exceptions;
using backend.Models;
using backend.Repository.Interfaces;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace backend.Services.Implementation
{
    public class PatternExcelService : IPatternExcelService
    {
        private const long MaxFileBytes = 2 * 1024 * 1024;
        private static readonly Regex InvalidFileNameChars = new(@"[\\/:*?""<>|\x00-\x1F]", RegexOptions.Compiled);

        private readonly IPatternRepository _patternRepository;
        private readonly IWorkAccessService _access;
        private readonly IYarnColorRepository _yarnColorRepository;
        private readonly IRealtimeOutbox _outbox;

        public PatternExcelService(
            IPatternRepository patternRepository,
            IWorkAccessService access,
            IYarnColorRepository yarnColorRepository,
            IRealtimeOutbox outbox)
        {
            _outbox = outbox;
            _patternRepository = patternRepository;
            _access = access;
            _yarnColorRepository = yarnColorRepository;
        }

        public async Task<ExcelFileResult> ExportAsync(Guid userId, Guid workId)
        {
            var work = (await _access.RequireAsync(userId, workId, WorkAccessLevel.Read)).Work;
            var pattern = await _patternRepository.GetByWorkIdAsync(workId)
                ?? throw new NotFoundException(ErrorCode.PatternNotFound);

            var cells = await _patternRepository.GetAllCellsAsync(pattern.Id);
            var content = ExcelMatrixWriter.Write(work.Name, pattern.Width, pattern.Height, cells);

            var safeName = InvalidFileNameChars.Replace(work.Name, "_").Trim();
            return new ExcelFileResult(content, (safeName.Length == 0 ? "matrix" : safeName) + ".xlsx");
        }

        public async Task<ImportPreviewResponse> PreviewImportAsync(Guid userId, Guid workId, IFormFile file)
        {
            await EnsureImportableAsync(userId, workId);
            return await PreviewFileAsync(userId, file);
        }

        public async Task<ImportPreviewResponse> PreviewFileAsync(Guid userId, IFormFile file)
        {
            var matrix = await ReadMatrixAsync(file);
            var palette = await _yarnColorRepository.GetActiveByOwnerAsync(userId);
            var byHex = palette.ToDictionary(c => c.HexValue.ToUpperInvariant(), c => c);

            return new ImportPreviewResponse
            {
                Width = matrix.Width,
                Height = matrix.Height,
                ColoredCells = matrix.Cells.Count,
                SkippedCells = matrix.SkippedCells,
                Colors = matrix.Cells
                    .GroupBy(c => c.Hex)
                    .Select(g => new ImportPreviewColor
                    {
                        Hex = g.Key,
                        ExistingName = byHex.GetValueOrDefault(g.Key)?.Name,
                        Count = g.Count(),
                    })
                    .OrderByDescending(c => c.Count)
                    .ToList(),
                Warnings = matrix.Warnings
                    .Select(w => new ImportPreviewWarning { Cell = w.Cell, Reason = w.Reason })
                    .ToList(),
            };
        }

        public async Task<PatternResponse> ImportAsync(Guid userId, Guid workId, IFormFile file)
        {
            await EnsureImportableAsync(userId, workId);
            var matrix = await ReadMatrixAsync(file);
            var palette = await _yarnColorRepository.GetActiveByOwnerAsync(userId);

            var colorsByHex = palette.ToDictionary(c => c.HexValue.ToUpperInvariant(), c => c);
            var takenNames = palette.Select(c => c.Name.ToLowerInvariant()).ToHashSet();
            var now = DateTime.UtcNow;

            foreach (var hex in matrix.Cells.Select(c => c.Hex).Distinct())
            {
                if (colorsByHex.ContainsKey(hex))
                {
                    continue;
                }

                var color = new YarnColor
                {
                    Id = Guid.NewGuid(),
                    Name = UniqueName(hex, takenNames),
                    HexValue = hex,
                    OwnerId = userId,
                    CreatedAt = now,
                };
                await _yarnColorRepository.AddAsync(color);
                colorsByHex[hex] = color;
            }

            var pattern = new Pattern
            {
                Id = Guid.NewGuid(),
                WorkId = workId,
                Width = matrix.Width,
                Height = matrix.Height,
                CurrentRow = 0,
                CurrentColumn = 0,
            };
            await _patternRepository.AddAsync(pattern);

            var cells = new List<PatternCell>(matrix.Cells.Count);
            foreach (var excelCell in matrix.Cells)
            {
                var color = colorsByHex[excelCell.Hex];
                var cell = new PatternCell
                {
                    Id = Guid.NewGuid(),
                    PatternId = pattern.Id,
                    RowIndex = excelCell.Row,
                    ColumnIndex = excelCell.Column,
                    YarnColorId = color.Id,
                    YarnColor = color,
                };
                cells.Add(cell);
                await _patternRepository.AddCellAsync(cell);
            }

            await _patternRepository.SaveChangesAsync();
            _outbox.Enqueue(n => n.PatternResetAsync(workId));

            return PatternService.ToResponse(pattern, cells);
        }

        private async Task EnsureImportableAsync(Guid userId, Guid workId)
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
        }

        private static async Task<ExcelMatrix> ReadMatrixAsync(IFormFile file)
        {
            if (file.Length == 0
                || !file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException(ErrorCode.ExcelFileInvalid);
            }

            if (file.Length > MaxFileBytes)
            {
                throw new BadRequestException(ErrorCode.ExcelFileTooLarge);
            }

            using var buffer = new MemoryStream();
            await file.CopyToAsync(buffer);

            var bytes = buffer.GetBuffer();
            if (buffer.Length < 4 || bytes[0] != 0x50 || bytes[1] != 0x4B)
            {
                throw new BadRequestException(ErrorCode.ExcelFileInvalid);
            }

            buffer.Position = 0;
            return ExcelMatrixReader.Read(buffer);
        }

        private static string UniqueName(string baseName, HashSet<string> takenNames)
        {
            var name = baseName;
            var suffix = 2;
            while (takenNames.Contains(name.ToLowerInvariant()))
            {
                name = $"{baseName} ({suffix++})";
            }

            takenNames.Add(name.ToLowerInvariant());
            return name;
        }
    }
}
