using System.ComponentModel.DataAnnotations;

namespace Labor.Models.Entities.System
{
    public class OTPVerification
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(15)]
        public string MobileNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(6)]
        public string OTPCode { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Purpose { get; set; } = string.Empty; // 'Registration', 'Login', 'PasswordReset'
        
        public DateTime ExpiresAt { get; set; }
        
        public bool IsUsed { get; set; } = false;
        
        public DateTime? VerifiedAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 