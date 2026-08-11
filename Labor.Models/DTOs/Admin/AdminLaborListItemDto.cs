namespace Labor.Models.DTOs.Admin
{
    public class AdminLaborListItemDto
    {
        public int LaborId { get; set; }
        public int UserId { get; set; }
        public string? MobileNumber { get; set; }
        public string? LaborName { get; set; }
        public string? LaborType { get; set; }
        public string? Specialization { get; set; }
        public decimal DailyRate { get; set; }
        public decimal Rating { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public string? AvailabilityStatus { get; set; }
    }
}