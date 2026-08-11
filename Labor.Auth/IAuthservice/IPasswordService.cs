namespace Labor.Auth.IAuthservice
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        string GenerateTemporaryPassword(int length = 8);
    }
} 