namespace backend.Services.Interfaces
{
    public interface IVerificationCodeService
    {
        string GenerateCode();
        string HashCode(string code);
    }
}
