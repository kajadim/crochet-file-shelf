using System.Security.Claims;
using backend.Dtos.Auth;
using backend.Exceptions;
using backend.Options;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private const string RefreshTokenCookieName = "refreshToken";

        private readonly IAuthService _authService;
        private readonly RefreshTokenOptions _refreshTokenOptions;

        public AuthController(IAuthService authService, IOptions<RefreshTokenOptions> refreshTokenOptions)
        {
            _authService = authService;
            _refreshTokenOptions = refreshTokenOptions.Value;
        }

        [HttpPost("register")]
        public async Task<ActionResult<MessageResponse>> Register(RegisterRequest request)
        {
            try
            {
                await _authService.RegisterAsync(request);
                return Ok(new MessageResponse { Message = "Verification code sent to your email." });
            }
            catch (EmailAlreadyExistsException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("verify-email")]
        public async Task<ActionResult<AuthResponse>> VerifyEmail(VerifyEmailRequest request)
        {
            try
            {
                var (response, rawRefreshToken) = await _authService.VerifyEmailAsync(request);
                SetRefreshTokenCookie(rawRefreshToken);
                return Ok(response);
            }
            catch (InvalidOrExpiredCodeException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("resend-verification")]
        public async Task<ActionResult<MessageResponse>> ResendVerification(ResendVerificationRequest request)
        {
            try
            {
                await _authService.ResendVerificationAsync(request);
                return Ok(new MessageResponse { Message = "Verification code sent to your email." });
            }
            catch (PendingRegistrationNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            try
            {
                var (response, rawRefreshToken) = await _authService.LoginAsync(request);
                SetRefreshTokenCookie(rawRefreshToken);
                return Ok(response);
            }
            catch (InvalidCredentialsException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh()
        {
            var rawRefreshToken = Request.Cookies[RefreshTokenCookieName];
            if (string.IsNullOrEmpty(rawRefreshToken))
            {
                return Unauthorized(new { message = "Refresh token is missing." });
            }

            try
            {
                var (response, newRawRefreshToken) = await _authService.RefreshAsync(rawRefreshToken);
                SetRefreshTokenCookie(newRawRefreshToken);
                return Ok(response);
            }
            catch (InvalidRefreshTokenException ex)
            {
                ClearRefreshTokenCookie();
                return Unauthorized(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = GetUserId();
            await _authService.LogoutAsync(userId);
            ClearRefreshTokenCookie();
            return NoContent();
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<MessageResponse>> ForgotPassword(ForgotPasswordRequest request)
        {
            // Uvek isti odgovor bez obzira da li email postoji - ne otkriva registrovane naloge.
            await _authService.ForgotPasswordAsync(request);
            return Ok(new MessageResponse { Message = "If that email is registered, a reset code has been sent." });
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<MessageResponse>> ResetPassword(ResetPasswordRequest request)
        {
            try
            {
                await _authService.ResetPasswordAsync(request);
                return Ok(new MessageResponse { Message = "Password has been reset. Please log in again." });
            }
            catch (InvalidOrExpiredCodeException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        private Guid GetUserId()
        {
            var subClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.Parse(subClaim!);
        }

        private void SetRefreshTokenCookie(string rawRefreshToken)
        {
            Response.Cookies.Append(RefreshTokenCookieName, rawRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/api/auth",
                Expires = DateTimeOffset.UtcNow.AddDays(_refreshTokenOptions.ExpiryDays),
            });
        }

        private void ClearRefreshTokenCookie()
        {
            Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
            {
                Path = "/api/auth",
            });
        }
    }
}
