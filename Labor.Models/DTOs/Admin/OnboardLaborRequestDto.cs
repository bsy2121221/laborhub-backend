using System.ComponentModel.DataAnnotations;

namespace Labor.Models.DTOs.Admin
{
    public class OnboardLaborRequestDto
    {
        [Required]
        [StringLength(15)]
        public string MobileNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public int LaborTypeId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal DailyRate { get; set; }

        [StringLength(255)]
        public string? Specialization { get; set; }

        [Range(0, 80)]
        public int ExperienceYears { get; set; }

        public int MaximumHourAvilablePerDay { get; set; }

        [StringLength(255)]
        public string Street { get; set; } = string.Empty;
        [StringLength(100)]
        public string City { get; set; } = string.Empty;
        [StringLength(100)]
        public string State { get; set; } = string.Empty;
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;
        [StringLength(20)]
        public string ZipCode { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        [StringLength(255)]
        public string ProfilePicture { get; set; } = string.Empty;
    }
}