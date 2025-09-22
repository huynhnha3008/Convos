

namespace Services.Interfaces
{
    public interface IOTPService
    {
        string GenerateOTP();
        void StoreOTP(string keylogin, string otp);
        Task<bool> ValidateOTP(string keylogin, string otp);
    }
}
