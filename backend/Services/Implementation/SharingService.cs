using System.Security.Cryptography;
using backend.Dtos.Sharing;
using backend.Exceptions;
using backend.Models;
using backend.Repository.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementation
{
    public class SharingService : ISharingService
    {
        private const string SharedFolderName = "Shared with me";
        private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int CodeLength = 8;

        private readonly ISharingRepository _sharingRepository;
        private readonly IWorkAccessService _access;
        private readonly INotificationService _notifications;
        private readonly IFolderRepository _folderRepository;
        private readonly IUserRepository _userRepository;

        public SharingService(
            ISharingRepository sharingRepository,
            IWorkAccessService access,
            INotificationService notifications,
            IFolderRepository folderRepository,
            IUserRepository userRepository)
        {
            _sharingRepository = sharingRepository;
            _access = access;
            _notifications = notifications;
            _folderRepository = folderRepository;
            _userRepository = userRepository;
        }

        public async Task<SharingResponse> GetAsync(Guid userId, Guid workId)
        {
            await _access.RequireAsync(userId, workId, WorkAccessLevel.Owner);

            var invitation = await _sharingRepository.GetInvitationByWorkAsync(workId);
            var members = await _sharingRepository.GetMembersAsync(workId);

            return new SharingResponse
            {
                Invitation = invitation is null ? null : ToResponse(invitation),
                Members = members.Select(ToResponse).ToList(),
            };
        }

        public async Task<InvitationResponse> SetInvitationAsync(Guid userId, Guid workId, SetInvitationRequest request)
        {
            await _access.RequireAsync(userId, workId, WorkAccessLevel.Owner);

            var invitation = await _sharingRepository.GetInvitationByWorkAsync(workId);
            var code = await GenerateUniqueCodeAsync();

            if (invitation is null)
            {
                invitation = new WorkInvitation
                {
                    Id = Guid.NewGuid(),
                    WorkId = workId,
                    CreatedAt = DateTime.UtcNow,
                };
                await _sharingRepository.AddInvitationAsync(invitation);
            }

            invitation.Code = code;
            invitation.Permission = request.Permission!.Value;
            invitation.IsActive = true;
            invitation.CreatedAt = DateTime.UtcNow;
            await _sharingRepository.SaveChangesAsync();

            return ToResponse(invitation);
        }

        public async Task RevokeInvitationAsync(Guid userId, Guid workId)
        {
            await _access.RequireAsync(userId, workId, WorkAccessLevel.Owner);

            var invitation = await _sharingRepository.GetInvitationByWorkAsync(workId);
            if (invitation is not null)
            {
                invitation.IsActive = false;
                await _sharingRepository.SaveChangesAsync();
            }
        }

        public async Task<MemberResponse> UpdateMemberAsync(
            Guid userId, Guid workId, Guid memberUserId, UpdateMemberRequest request)
        {
            await _access.RequireAsync(userId, workId, WorkAccessLevel.Owner);

            var member = await GetMemberWithUserAsync(workId, memberUserId);
            member.Permission = request.Permission!.Value;
            await _sharingRepository.SaveChangesAsync();

            return ToResponse(member);
        }

        public async Task RemoveMemberAsync(Guid userId, Guid workId, Guid memberUserId)
        {
            var access = await _access.RequireAsync(userId, workId, WorkAccessLevel.Owner);

            var member = await GetMemberWithUserAsync(workId, memberUserId);
            _sharingRepository.RemoveMember(member);

            await _notifications.AddAsync(
                memberUserId, NotificationType.RemovedFromWork, workId, new { workName = access.Work.Name });
            await _sharingRepository.SaveChangesAsync();
        }

        public async Task LeaveAsync(Guid userId, Guid workId)
        {
            var member = await _sharingRepository.GetMemberAsync(workId, userId)
                ?? throw new NotFoundException(ErrorCode.WorkNotFound);

            _sharingRepository.RemoveMember(member);
            await _sharingRepository.SaveChangesAsync();
        }

        public async Task<JoinWorkResponse> JoinAsync(Guid userId, JoinWorkRequest request)
        {
            var code = NormalizeCode(request.Code);
            if (code.Length != CodeLength)
            {
                throw new NotFoundException(ErrorCode.InvitationInvalid);
            }

            var invitation = await _sharingRepository.GetActiveInvitationByCodeAsync(code)
                ?? throw new NotFoundException(ErrorCode.InvitationInvalid);
            var work = invitation.Work;

            if (work.OwnerId == userId)
            {
                throw new ConflictException(ErrorCode.CannotJoinOwnWork);
            }
            if (await _sharingRepository.GetMemberAsync(work.Id, userId) is not null)
            {
                throw new ConflictException(ErrorCode.AlreadyWorkMember);
            }

            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException(ErrorCode.InvitationInvalid);

            await _sharingRepository.AddMemberAsync(new WorkMember
            {
                Id = Guid.NewGuid(),
                WorkId = work.Id,
                UserId = userId,
                Permission = invitation.Permission,
                JoinedAt = DateTime.UtcNow,
            });

            await _notifications.AddAsync(
                userId, NotificationType.WorkInvite, work.Id,
                new { workName = work.Name, permission = invitation.Permission.ToString() });
            await _notifications.AddAsync(
                work.OwnerId, NotificationType.MemberJoined, work.Id,
                new { workName = work.Name, userName = user.DisplayName });
            await _sharingRepository.SaveChangesAsync();

            return new JoinWorkResponse
            {
                WorkId = work.Id,
                Name = work.Name,
                Type = work.Type,
                Permission = invitation.Permission,
            };
        }

        public async Task<bool> PrepareRemovalAsync(Work work)
        {
            var members = await _sharingRepository.GetMembersAsync(work.Id);
            if (members.Count == 0)
            {
                return false;
            }

            var successor = members
                .Where(m => m.Permission == WorkPermission.CanEdit)
                .OrderBy(m => m.JoinedAt)
                .FirstOrDefault();

            if (successor is null)
            {
                foreach (var member in members)
                {
                    await _notifications.AddAsync(
                        member.UserId, NotificationType.WorkDeleted, null, new { workName = work.Name });
                }
                return false;
            }

            var folder = await EnsureSharedFolderAsync(successor.UserId);
            work.OwnerId = successor.UserId;
            work.FolderId = folder.Id;
            work.UpdatedAt = DateTime.UtcNow;

            foreach (var member in members)
            {
                if (member.Id == successor.Id)
                {
                    _sharingRepository.RemoveMember(member);
                }
                else if (member.Permission == WorkPermission.ViewOnly)
                {
                    _sharingRepository.RemoveMember(member);
                    await _notifications.AddAsync(
                        member.UserId, NotificationType.WorkDeleted, null, new { workName = work.Name });
                }
            }

            var invitation = await _sharingRepository.GetInvitationByWorkAsync(work.Id);
            if (invitation is not null)
            {
                _sharingRepository.RemoveInvitation(invitation);
            }

            await _notifications.AddAsync(
                successor.UserId, NotificationType.OwnershipTransferred, work.Id, new { workName = work.Name });

            return true;
        }

        private async Task<Folder> EnsureSharedFolderAsync(Guid ownerId)
        {
            var existing = await _folderRepository.GetRootByNameAsync(ownerId, SharedFolderName);
            if (existing is not null)
            {
                return existing;
            }

            var folder = new Folder
            {
                Id = Guid.NewGuid(),
                Name = SharedFolderName,
                OwnerId = ownerId,
                ParentFolderId = null,
                CreatedAt = DateTime.UtcNow,
            };
            await _folderRepository.AddAsync(folder);
            return folder;
        }

        private async Task<WorkMember> GetMemberWithUserAsync(Guid workId, Guid memberUserId)
        {
            var members = await _sharingRepository.GetMembersAsync(workId);
            return members.FirstOrDefault(m => m.UserId == memberUserId)
                ?? throw new NotFoundException(ErrorCode.MemberNotFound);
        }

        private async Task<string> GenerateUniqueCodeAsync()
        {
            for (var attempt = 0; attempt < 10; attempt++)
            {
                var code = string.Create(CodeLength, 0, (span, _) =>
                {
                    for (var i = 0; i < span.Length; i++)
                    {
                        span[i] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];
                    }
                });

                if (!await _sharingRepository.CodeExistsAsync(code))
                {
                    return code;
                }
            }

            throw new InvalidOperationException("Could not generate a unique invitation code.");
        }

        private static string NormalizeCode(string input) =>
            new string(input.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

        private static InvitationResponse ToResponse(WorkInvitation invitation) => new()
        {
            Code = invitation.Code.Length == CodeLength
                ? invitation.Code[..4] + "-" + invitation.Code[4..]
                : invitation.Code,
            Permission = invitation.Permission,
            IsActive = invitation.IsActive,
        };

        private static MemberResponse ToResponse(WorkMember member) => new()
        {
            UserId = member.UserId,
            DisplayName = member.User.DisplayName,
            Permission = member.Permission,
            JoinedAt = member.JoinedAt,
        };
    }
}
