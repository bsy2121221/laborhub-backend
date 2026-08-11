namespace Labor.Models.DTOs.Admin;

public class UserProfileDetailDto
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsProfileComplete { get; set; }
    public bool IsMobileVerified { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? ProfilePicture { get; set; }
    public int? AddressId { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
    public decimal? AddressLatitude { get; set; }
    public decimal? AddressLongitude { get; set; }
    public int? LaborId { get; set; }
    public string? LaborTypeName { get; set; }
    public string? Specialization { get; set; }
    public int? ExperienceYears { get; set; }
    public decimal? DailyRate { get; set; }
    public int? MinimumHours { get; set; }
    public int? MaximumHours { get; set; }
    public string? AvailabilityStatus { get; set; }
    public bool? LaborIsVerified { get; set; }
    public bool? LaborIsActive { get; set; }
    public bool CanEditUsers { get; set; }
}