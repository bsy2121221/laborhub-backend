

namespace Labor.Models.DTOs.Admin
{
    public class AdminLaborDetailDto
    {
        public int LaborId { get; set; }
        public int UserId { get; set; }
        public string? MobileNumber { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? ProfilePicture { get; set; }
        public int LaborTypeId { get; set; }
        public string? LaborTypeName { get; set; }
        public string? Specialization { get; set; }
        public int ExperienceYears { get; set; }
        public decimal DailyRate { get; set; }
        public int MinimumHours { get; set; }
        public int MaximumHours { get; set; }
        public string? AvailabilityStatus { get; set; }
        public bool IsVerified { get; set; }
        public bool LaborListingActive { get; set; }
        public int AddressId { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? ZipCode { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
