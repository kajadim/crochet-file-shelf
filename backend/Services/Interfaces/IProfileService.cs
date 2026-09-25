using backend.Dtos.Profile;
using Microsoft.AspNetCore.Http;

namespace backend.Services.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileResponse> GetAsync(Guid userId);
        Task<ProfileResponse> UpdateAsync(Guid userId, UpdateProfileRequest request);
        Task<ProfileResponse> SetAvatarAsync(Guid userId, IFormFile file);
        Task<ProfileResponse> RemoveAvatarAsync(Guid userId);
        Task<AvatarFile?> GetAvatarAsync(Guid userId);
    }
}
