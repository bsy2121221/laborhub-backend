using System.ComponentModel.DataAnnotations;

namespace Labor.Models.DTOs.Auth
{
    public class OTPRequestDto
    {
        [Required]
        [StringLength(15, MinimumLength = 10)]
        public string MobileNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Purpose { get; set; } = string.Empty; // 'Registration', 'Login', 'PasswordReset'
    }
} 