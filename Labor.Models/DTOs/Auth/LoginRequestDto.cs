using System.ComponentModel.DataAnnotations;

namespace Labor.Models.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required]
        [StringLength(15, MinimumLength = 10)]
        public string MobileNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }
} 