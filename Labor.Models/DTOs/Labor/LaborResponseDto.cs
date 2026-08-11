namespace Labor.Models.DTOs.Labor
{
    public class LaborResponseDto
    {
        public int LaborId { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public int LaborTypeId { get; set; }
        public string LaborType { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public int ExperienceYears { get; set; }
        public decimal Rating { get; set; }
        public int TotalReviews { get; set; }
        public decimal DailyRate { get; set; }
        public int MinimumHours { get; set; }
        public int MaximumHours { get; set; }
        public string AvailabilityStatus { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public decimal? DistanceKm { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public DateTime AvailableDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public List<LaborSkillDto> Skills { get; set; } = new();
        public List<LaborReviewDto> RecentReviews { get; set; } = new();
    }
} 