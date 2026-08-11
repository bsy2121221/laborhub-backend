namespace Labor.Models.DTOs.Auth
{
    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime TokenExpiry { get; set; }
        public bool IsTemporaryPassword { get; set; }
        public bool RequirePasswordChange { get; set; }
        public bool IsProfileComplete { get; set; }
    }
} 