using backend.Dtos.Auth;

namespace backend.Services.Interfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequest request);
        Task<(AuthResponse Response, string RawRefreshToken)> VerifyEmailAsync(VerifyEmailRequest request);
        Task ResendVerificationAsync(ResendVerificationRequest request);
        Task<(AuthResponse Response, string RawRefreshToken)> LoginAsync(LoginRequest request);
        Task<(AuthResponse Response, string RawRefreshToken)> RefreshAsync(string rawRefreshToken);
        Task LogoutAsync(Guid userId);
        Task ForgotPasswordAsync(ForgotPasswordRequest request);
        Task ResetPasswordAsync(ResetPasswordRequest request);
    }
}
