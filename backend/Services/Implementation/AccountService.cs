using backend.Dtos.Profile;
using backend.Exceptions;
using backend.Models;
using backend.Repository.Interfaces;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace backend.Services.Implementation
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        private readonly IWorkRepository _workRepository;
        private readonly IYarnColorRepository _yarnColorRepository;
        private readonly IWorkService _workService;
        private readonly ISharingService _sharingService;
        private readonly IPatternColorService _patternColors;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountService(
            IUserRepository userRepository,
            IWorkRepository workRepository,
            IYarnColorRepository yarnColorRepository,
            IWorkService workService,
            ISharingService sharingService,
            IPatternColorService patternColors,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _workRepository = workRepository;
            _yarnColorRepository = yarnColorRepository;
            _workService = workService;
            _sharingService = sharingService;
            _patternColors = patternColors;
            _passwordHasher = passwordHasher;
        }

        public async Task DeleteAsync(Guid userId, DeleteAccountRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException(ErrorCode.WorkNotFound);

            if (_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            {
                throw new BadRequestException(ErrorCode.IncorrectPassword);
            }

            await using var transaction = await _userRepository.BeginTransactionAsync();

            foreach (var workId in await _workRepository.GetOwnedIdsAsync(userId))
            {
                await _workService.DeleteAsync(userId, workId);
            }

            foreach (var workId in await _workRepository.GetMemberWorkIdsAsync(userId))
            {
                await _sharingService.LeaveAsync(userId, workId);
            }

            foreach (var (workId, ownerId) in await _yarnColorRepository.GetWorksUsingColorsOfAsync(userId))
            {
                await _patternColors.MoveColorsToOwnerAsync(workId, ownerId, userId);
                await _yarnColorRepository.SaveChangesAsync();
            }

            _userRepository.Remove(user);
            await _userRepository.SaveChangesAsync();

            await transaction.CommitAsync();
        }
    }
}
