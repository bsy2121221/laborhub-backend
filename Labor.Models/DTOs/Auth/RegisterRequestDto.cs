using System.ComponentModel.DataAnnotations;

namespace Labor.Models.DTOs.Auth
{
    /// <summary>
    /// Public self-service registration: employers only (mobile + password).
    /// Name and address are collected later (e.g. checkout). Labor accounts are created by admins.
    /// </summary>
    public class RegisterRequestDto
    {
        [Required]
        [StringLength(15, MinimumLength = 10)]
        public string MobileNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }
} 