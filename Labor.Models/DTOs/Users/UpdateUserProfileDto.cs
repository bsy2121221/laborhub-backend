using System.ComponentModel.DataAnnotations;

namespace Labor.Models.DTOs.Admin;

public class UpdateUserProfileDto
{
    [StringLength(100)]
    public string? UserName { get; set; }

    [StringLength(15)]
    public string? MobileNumber { get; set; }

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(100, MinimumLength = 6)]
    public string? Password { get; set; }

    [StringLength(100, MinimumLength = 6)]
    public string? NewPassword { get; set; }

    [Compare("NewPassword", ErrorMessage = "Password confirmation does not match.")]
    public string? ConfirmPassword { get; set; }

    [StringLength(500)]
    public string? ProfilePicture { get; set; }
}