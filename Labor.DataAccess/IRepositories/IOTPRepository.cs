using Labor.Models.Entities.System;

namespace Labor.DataAccess.IRepositories
{
    public interface IOTPRepository
    {
        Task<bool> CreateOTPAsync(string mobileNumber, string otpCode, string purpose, int expiryMinutes = 5);
        Task<bool> VerifyOTPAsync(string mobileNumber, string otpCode, string purpose);
    }
} 