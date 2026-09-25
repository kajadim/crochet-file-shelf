using backend.Exceptions;
using backend.Models;
using backend.Repository.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementation
{
    public class WorkAccessService : IWorkAccessService
    {
        private readonly IWorkRepository _workRepository;

        public WorkAccessService(IWorkRepository workRepository)
        {
            _workRepository = workRepository;
        }

        public async Task<WorkAccess> RequireAsync(Guid userId, Guid workId, WorkAccessLevel level)
        {
            var work = await _workRepository.GetByIdAsync(workId)
                ?? throw new NotFoundException(ErrorCode.WorkNotFound);

            WorkRole role;
            if (work.OwnerId == userId)
            {
                role = WorkRole.Owner;
            }
            else
            {
                var member = await _workRepository.GetMemberAsync(workId, userId)
                    ?? throw new NotFoundException(ErrorCode.WorkNotFound);
                role = member.Permission == WorkPermission.CanEdit ? WorkRole.Editor : WorkRole.Viewer;
            }

            var allowed = level switch
            {
                WorkAccessLevel.Read => true,
                WorkAccessLevel.Edit => role is WorkRole.Owner or WorkRole.Editor,
                _ => role == WorkRole.Owner,
            };

            if (!allowed)
            {
                throw new ForbiddenException(ErrorCode.WorkAccessDenied);
            }

            return new WorkAccess(work, role);
        }
    }
}
