using Labor.Auth.IAuthservice;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Labor.Auth.Services
{
    public class OTPService : IOTPService
    {
        private readonly ILogger<OTPService> _logger;

        public OTPService(ILogger<OTPService> logger)
        {
            _logger = logger;
        }

        public string GenerateOTP(int length = 6)
        {
            using var rng = RandomNumberGenerator.Create();
            var randomBytes = new byte[length];
            rng.GetBytes(randomBytes);
            
            var otp = string.Empty;
            for (int i = 0; i < length; i++)
            {
                otp += (randomBytes[i] % 10).ToString();
            }
            
            return otp;
        }

        public async Task<bool> SendOTPAsync(string mobileNumber, string otp, string purpose)
        {
            // TODO: Implement actual SMS sending logic here
            // For now, just log the OTP (in production, integrate with SMS gateway)
            _logger.LogInformation($"OTP for {mobileNumber} ({purpose}): {otp}");
            
            // Simulate async operation
            await Task.Delay(100);
            
            // Return true to simulate successful sending
            // In production, return actual result from SMS gateway
            return true;
        }
    }
} 