using System.ComponentModel.DataAnnotations;

namespace Labor.Models.DTOs.Auth
{
    public class ChangePasswordDto
    {
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string CurrentPassword { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;
        
        [Required]
        [Compare("NewPassword", ErrorMessage = "Password confirmation does not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
} 