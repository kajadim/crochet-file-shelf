using backend.Dtos.Profile;

namespace backend.Services.Interfaces
{
    public interface IAccountService
    {
        Task DeleteAsync(Guid userId, DeleteAccountRequest request);
    }
}
