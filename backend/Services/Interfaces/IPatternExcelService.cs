using backend.Dtos.Patterns;
using Microsoft.AspNetCore.Http;

namespace backend.Services.Interfaces
{
    public interface IPatternExcelService
    {
        Task<ExcelFileResult> ExportAsync(Guid userId, Guid workId);
        Task<ImportPreviewResponse> PreviewImportAsync(Guid userId, Guid workId, IFormFile file);
        Task<PatternResponse> ImportAsync(Guid userId, Guid workId, IFormFile file);
    }
}
