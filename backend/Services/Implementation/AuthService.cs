using backend.Dtos.Auth;
using backend.Exceptions;
using backend.Models;
using backend.Options;
using backend.Repository.Interfaces;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace backend.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private static readonly TimeSpan VerificationCodeLifetime = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan PasswordResetCodeLifetime = TimeSpan.FromMinutes(15);

        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPendingRegistrationRepository _pendingRegistrationRepository;
        private readonly IPasswordResetCodeRepository _passwordResetCodeRepository;
        private readonly ITokenService _tokenService;
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly IEmailSender _emailSender;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly RefreshTokenOptions _refreshTokenOptions;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPendingRegistrationRepository pendingRegistrationRepository,
            IPasswordResetCodeRepository passwordResetCodeRepository,
            ITokenService tokenService,
            IVerificationCodeService verificationCodeService,
            IEmailSender emailSender,
            IPasswordHasher<User> passwordHasher,
            IOptions<RefreshTokenOptions> refreshTokenOptions)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _pendingRegistrationRepository = pendingRegistrationRepository;
            _passwordResetCodeRepository = passwordResetCodeRepository;
            _tokenService = tokenService;
            _verificationCodeService = verificationCodeService;
            _emailSender = emailSender;
            _passwordHasher = passwordHasher;
            _refreshTokenOptions = refreshTokenOptions.Value;
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            if (await _userRepository.EmailExistsAsync(normalizedEmail))
            {
                throw new EmailAlreadyExistsException();
            }

            // Heš lozinke se pravi nad "praznim" korisnikom - PasswordHasher ne koristi ništa
            // od stvarnih podataka korisnika, samo mu je potreban instance parametar.
            var username = UsernameRules.Normalize(request.Username);
            if (await _userRepository.UsernameExistsAsync(username, null)
                || await _pendingRegistrationRepository.UsernameReservedAsync(username, normalizedEmail))
            {
                throw new ConflictException(ErrorCode.UsernameTaken);
            }

            var passwordHash = _passwordHasher.HashPassword(new User(), request.Password);

            var existingPending = await _pendingRegistrationRepository.GetByEmailAsync(normalizedEmail);
            if (existingPending is not null)
            {
                _pendingRegistrationRepository.Remove(existingPending);
            }

            var code = _verificationCodeService.GenerateCode();

            var pendingRegistration = new PendingRegistration
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                PasswordHash = passwordHash,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Username = username,
                CodeHash = _verificationCodeService.HashCode(code),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(VerificationCodeLifetime),
            };

            await _pendingRegistrationRepository.AddAsync(pendingRegistration);
            await _pendingRegistrationRepository.SaveChangesAsync();

            await _emailSender.SendAsync(
                normalizedEmail,
                "Confirm your Crochet File Shelf account",
                $"<p>Your verification code is:</p><h2>{code}</h2><p>This code expires in 15 minutes.</p>");
        }

        public async Task<(AuthResponse Response, string RawRefreshToken)> VerifyEmailAsync(VerifyEmailRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var pending = await _pendingRegistrationRepository.GetByEmailAsync(normalizedEmail);

            if (pending is null || pending.ExpiresAt <= DateTime.UtcNow)
            {
                throw new InvalidOrExpiredCodeException();
            }

            var submittedCodeHash = _verificationCodeService.HashCode(request.Code);
            if (submittedCodeHash != pending.CodeHash)
            {
                throw new InvalidOrExpiredCodeException();
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = pending.Email,
                PasswordHash = pending.PasswordHash,
                FirstName = pending.FirstName,
                LastName = pending.LastName,
                Username = pending.Username,
                CreatedAt = DateTime.UtcNow,
            };

            if (await _userRepository.UsernameExistsAsync(user.Username, null))
            {
                throw new ConflictException(ErrorCode.UsernameTaken);
            }

            await _userRepository.AddAsync(user);
            _pendingRegistrationRepository.Remove(pending);
            await _userRepository.SaveChangesAsync();

            return await IssueTokensAsync(user);
        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            var normalized = UsernameRules.Normalize(username);
            if (!UsernameRules.IsValid(normalized))
            {
                return false;
            }

            return !await _userRepository.UsernameExistsAsync(normalized, null)
                && !await _pendingRegistrationRepository.UsernameReservedAsync(normalized, string.Empty);
        }

        public async Task ResendVerificationAsync(ResendVerificationRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var pending = await _pendingRegistrationRepository.GetByEmailAsync(normalizedEmail);

            if (pending is null)
            {
                throw new PendingRegistrationNotFoundException();
            }

            var code = _verificationCodeService.GenerateCode();
            pending.CodeHash = _verificationCodeService.HashCode(code);
            pending.ExpiresAt = DateTime.UtcNow.Add(VerificationCodeLifetime);
            await _pendingRegistrationRepository.SaveChangesAsync();

            await _emailSender.SendAsync(
                normalizedEmail,
                "Your new Crochet File Shelf verification code",
                $"<p>Your verification code is:</p><h2>{code}</h2><p>This code expires in 15 minutes.</p>");
        }

        public async Task<(AuthResponse Response, string RawRefreshToken)> LoginAsync(LoginRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);

            if (user is null)
            {
                throw new InvalidCredentialsException();
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                throw new InvalidCredentialsException();
            }

            return await IssueTokensAsync(user);
        }

        public async Task<(AuthResponse Response, string RawRefreshToken)> RefreshAsync(string rawRefreshToken)
        {
            var tokenHash = _tokenService.HashRefreshToken(rawRefreshToken);
            var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            if (existingToken is null || existingToken.RevokedAt is not null || existingToken.ExpiresAt <= DateTime.UtcNow)
            {
                throw new InvalidRefreshTokenException();
            }

            var user = await _userRepository.GetByIdAsync(existingToken.UserId);
            if (user is null)
            {
                throw new InvalidRefreshTokenException();
            }

            var (response, newRawRefreshToken, newRefreshTokenEntity) = await IssueTokensInternalAsync(user);

            // Rotacija: stari token se opoziva i povezuje sa novim (Faza 0 odluka)
            existingToken.RevokedAt = DateTime.UtcNow;
            existingToken.ReplacedByTokenId = newRefreshTokenEntity.Id;
            await _refreshTokenRepository.SaveChangesAsync();

            return (response, newRawRefreshToken);
        }

        public async Task LogoutAsync(Guid userId)
        {
            // Logout opoziva SVE aktivne refresh tokene korisnika (Faza 0 odluka)
            await _refreshTokenRepository.RevokeAllActiveForUserAsync(userId);
            await _refreshTokenRepository.SaveChangesAsync();
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);

            // Namerno se ne otkriva da li email postoji - isti odgovor u oba slucaja (poziva se iz kontrolera).
            if (user is null)
            {
                return;
            }

            await _passwordResetCodeRepository.RemoveAllForUserAsync(user.Id);

            var code = _verificationCodeService.GenerateCode();
            var resetCode = new PasswordResetCode
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CodeHash = _verificationCodeService.HashCode(code),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(PasswordResetCodeLifetime),
            };
            await _passwordResetCodeRepository.AddAsync(resetCode);
            await _passwordResetCodeRepository.SaveChangesAsync();

            await _emailSender.SendAsync(
                normalizedEmail,
                "Reset your Crochet File Shelf password",
                $"<p>Your password reset code is:</p><h2>{code}</h2><p>This code expires in 15 minutes. If you didn't request this, you can ignore this email.</p>");
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);

            if (user is null)
            {
                throw new InvalidOrExpiredCodeException();
            }

            var resetCode = await _passwordResetCodeRepository.GetActiveByUserIdAsync(user.Id);
            if (resetCode is null || resetCode.ExpiresAt <= DateTime.UtcNow)
            {
                throw new InvalidOrExpiredCodeException();
            }

            var submittedCodeHash = _verificationCodeService.HashCode(request.Code);
            if (submittedCodeHash != resetCode.CodeHash)
            {
                throw new InvalidOrExpiredCodeException();
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            resetCode.UsedAt = DateTime.UtcNow;

            // Promena lozinke opoziva sve refresh tokene - korisnik se mora ponovo ulogovati (Faza 0 odluka)
            await _refreshTokenRepository.RevokeAllActiveForUserAsync(user.Id);

            await _userRepository.SaveChangesAsync();
            await _passwordResetCodeRepository.SaveChangesAsync();
            await _refreshTokenRepository.SaveChangesAsync();
        }

        private async Task<(AuthResponse Response, string RawRefreshToken)> IssueTokensAsync(User user)
        {
            var (response, rawRefreshToken, _) = await IssueTokensInternalAsync(user);
            return (response, rawRefreshToken);
        }

        private async Task<(AuthResponse Response, string RawRefreshToken, RefreshToken RefreshTokenEntity)> IssueTokensInternalAsync(User user)
        {
            var (accessToken, accessTokenExpiresAt) = _tokenService.GenerateAccessToken(user);
            var rawRefreshToken = _tokenService.GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = _tokenService.HashRefreshToken(rawRefreshToken),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenOptions.ExpiryDays),
            };
            await _refreshTokenRepository.AddAsync(refreshToken);
            await _refreshTokenRepository.SaveChangesAsync();

            var response = new AuthResponse
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,
                User = new UserSummary
                {
                    Id = user.Id,
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    Username = user.Username,
                    AvatarVersion = user.AvatarVersion,
                },
            };

            return (response, rawRefreshToken, refreshToken);
        }
    }
}
