namespace Labor.Auth.IAuthservice
{
    public interface IOTPService
    {
        string GenerateOTP(int length = 6);
        Task<bool> SendOTPAsync(string mobileNumber, string otp, string purpose);
    }
} 