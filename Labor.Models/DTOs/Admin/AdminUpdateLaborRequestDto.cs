using System.ComponentModel.DataAnnotations;

namespace Labor.Models.DTOs.Admin
{
    public class AdminUpdateLaborRequestDto
    {
        [Required]
        [StringLength(15)]
        public string MobileNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? UserName { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? ProfilePicture { get; set; }

        /// <summary>Optional. Omit or leave empty to keep current password.</summary>
        [MinLength(6)]
        public string? NewPassword { get; set; }

        [Required]
        public int LaborTypeId { get; set; }

        [StringLength(255)]
        public string? Specialization { get; set; }

        [Range(0, 80)]
        public int ExperienceYears { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal DailyRate { get; set; }

        [Range(1, 168)]
        public int MinimumHours { get; set; } = 1;

        [Range(1, 168)]
        public int MaximumHours { get; set; } = 24;

        [StringLength(50)]
        public string AvailabilityStatus { get; set; } = "Available";

        [Required]
        [StringLength(255)]
        public string Street { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ZipCode { get; set; } = string.Empty;

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}