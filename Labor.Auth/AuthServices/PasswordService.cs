using BCrypt.Net;
using Labor.Auth.IAuthservice;
using System.Security.Cryptography;
using System.Text;

namespace Labor.Auth.Services
{
    public class PasswordService : IPasswordService
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        public string GenerateTemporaryPassword(int length = 8)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*";
            using var rng = RandomNumberGenerator.Create();
            var result = new StringBuilder(length);
            
            for (int i = 0; i < length; i++)
            {
                var randomBytes = new byte[1];
                rng.GetBytes(randomBytes);
                var randomIndex = randomBytes[0] % validChars.Length;
                result.Append(validChars[randomIndex]);
            }
            
            return result.ToString();
        }
    }
} 