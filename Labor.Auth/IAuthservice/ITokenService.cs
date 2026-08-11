using Labor.Models.Entities.User;

namespace Labor.Auth.IAuthservice
{
    public interface ITokenService
    {
        string GenerateToken(User user, string role);
        bool ValidateToken(string token);
        int GetUserIdFromToken(string token);
        string GetRoleFromToken(string token);
        DateTime GetTokenExpiry(string token);
    }
} 