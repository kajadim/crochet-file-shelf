using backend.Dtos.Profile;
using backend.Exceptions;
using backend.Models;
using backend.Repository.Interfaces;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace backend.Services.Implementation
{
    public class ProfileService : IProfileService
    {
        private const long MaxAvatarBytes = 300 * 1024;

        private readonly IUserRepository _userRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IPendingRegistrationRepository _pendingRepository;

        public ProfileService(
            IUserRepository userRepository,
            IProfileRepository profileRepository,
            IPendingRegistrationRepository pendingRepository)
        {
            _userRepository = userRepository;
            _profileRepository = profileRepository;
            _pendingRepository = pendingRepository;
        }

        public async Task<ProfileResponse> GetAsync(Guid userId)
        {
            var user = await GetUserAsync(userId);

            var counts = await _profileRepository.GetWorkCountsAsync(userId);
            var owned = await _profileRepository.GetMembersOfOwnedWorksAsync(userId);
            var memberships = await _profileRepository.GetMembershipsAsync(userId);

            return new ProfileResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DisplayName = user.DisplayName,
                Username = user.Username,
                Bio = user.Bio,
                AvatarVersion = user.AvatarVersion,
                CreatedAt = user.CreatedAt,
                WorkCounts = new WorkCounts
                {
                    Pattern = counts.GetValueOrDefault(WorkType.Pattern),
                    Video = counts.GetValueOrDefault(WorkType.Video),
                    Site = counts.GetValueOrDefault(WorkType.Site),
                },
                SharedByMe = Group(owned, m => m.User),
                SharedWithMe = Group(memberships, m => m.Work.Owner),
            };
        }

        public async Task<ProfileResponse> UpdateAsync(Guid userId, UpdateProfileRequest request)
        {
            var user = await GetUserAsync(userId);
            var username = UsernameRules.Normalize(request.Username);

            if (username != user.Username
                && (await _userRepository.UsernameExistsAsync(username, userId)
                    || await _pendingRepository.UsernameReservedAsync(username, string.Empty)))
            {
                throw new ConflictException(ErrorCode.UsernameTaken);
            }

            var bio = request.Bio?.Trim();

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.Username = username;
            user.Bio = string.IsNullOrEmpty(bio) ? null : bio;
            await _userRepository.SaveChangesAsync();

            return await GetAsync(userId);
        }

        public async Task<ProfileResponse> SetAvatarAsync(Guid userId, IFormFile file)
        {
            if (file.Length == 0)
            {
                throw new BadRequestException(ErrorCode.AvatarInvalid);
            }
            if (file.Length > MaxAvatarBytes)
            {
                throw new BadRequestException(ErrorCode.AvatarTooLarge);
            }

            using var buffer = new MemoryStream();
            await file.CopyToAsync(buffer);
            var bytes = buffer.ToArray();

            if (bytes.Length < 3 || bytes[0] != 0xFF || bytes[1] != 0xD8 || bytes[2] != 0xFF)
            {
                throw new BadRequestException(ErrorCode.AvatarInvalid);
            }

            var user = await GetUserAsync(userId);
            await _profileRepository.SetAvatarAsync(userId, bytes);
            user.AvatarUpdatedAt = DateTime.UtcNow;
            await _profileRepository.SaveChangesAsync();

            return await GetAsync(userId);
        }

        public async Task<ProfileResponse> RemoveAvatarAsync(Guid userId)
        {
            var user = await GetUserAsync(userId);
            await _profileRepository.RemoveAvatarAsync(userId);
            user.AvatarUpdatedAt = null;
            await _profileRepository.SaveChangesAsync();

            return await GetAsync(userId);
        }

        public async Task<AvatarFile?> GetAvatarAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user?.AvatarVersion is null)
            {
                return null;
            }

            var bytes = await _profileRepository.GetAvatarAsync(userId);
            return bytes is null ? null : new AvatarFile(bytes, user.AvatarVersion.Value);
        }

        private async Task<User> GetUserAsync(Guid userId) =>
            await _userRepository.GetByIdAsync(userId) ?? throw new NotFoundException(ErrorCode.WorkNotFound);

        private static List<ProfileCollaborator> Group(IEnumerable<WorkMember> members, Func<WorkMember, User> person) =>
            members
                .GroupBy(m => person(m).Id)
                .Select(group =>
                {
                    var user = person(group.First());
                    return new ProfileCollaborator
                    {
                        UserId = user.Id,
                        DisplayName = user.DisplayName,
                        Username = user.Username,
                        AvatarVersion = user.AvatarVersion,
                        Works = group
                            .OrderBy(m => m.Work.Name)
                            .Select(m => new ProfileSharedWork
                            {
                                WorkId = m.WorkId,
                                Name = m.Work.Name,
                                Type = m.Work.Type,
                                Permission = m.Permission,
                            })
                            .ToList(),
                    };
                })
                .OrderBy(c => c.DisplayName)
                .ToList();
    }
}
