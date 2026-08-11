using System.ComponentModel.DataAnnotations;

namespace Labor.Models.DTOs.Labor
{
    public class LaborSearchRequestDto
    {
        [Required]
        public string? availabilityDate { get; set; }
        public int? LaborTypeId { get; set; }
        public string? SearchText { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int RadiusKm { get; set; } = 50;
        public decimal MinRating { get; set; } = 0;
        public decimal? MaxDailyRate { get; set; }
        public string AvailabilityStatus { get; set; } = "Available";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
} 