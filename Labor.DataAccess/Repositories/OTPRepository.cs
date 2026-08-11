using Dapper;
using Labor.DataAccess.Context;
using Labor.DataAccess.IRepositories;
using System.Data;

namespace Labor.DataAccess.Repositories
{
    public class OTPRepository : IOTPRepository
    {
        private readonly IDbContext _context;

        public OTPRepository(IDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateOTPAsync(string mobileNumber, string otpCode, string purpose, int expiryMinutes = 5)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@MobileNumber", mobileNumber);
            parameters.Add("@OTPCode", otpCode);
            parameters.Add("@Purpose", purpose);
            parameters.Add("@ExpiryMinutes", expiryMinutes);

            await connection.ExecuteAsync(
                "[System].[sp_CreateOTP]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return true;
        }

        public async Task<bool> VerifyOTPAsync(string mobileNumber, string otpCode, string purpose)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@MobileNumber", mobileNumber);
            parameters.Add("@OTPCode", otpCode);
            parameters.Add("@Purpose", purpose);

            var result = await connection.QuerySingleAsync<bool>(
                "[System].[sp_VerifyOTP]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }
    }
} 