using backend.Models;

namespace backend.Services.Interfaces
{
    public enum WorkAccessLevel
    {
        Read,
        Edit,
        Owner,
    }

    public sealed record WorkAccess(Work Work, WorkRole Role);

    public interface IWorkAccessService
    {
        Task<WorkAccess> RequireAsync(Guid userId, Guid workId, WorkAccessLevel level);
    }
}
