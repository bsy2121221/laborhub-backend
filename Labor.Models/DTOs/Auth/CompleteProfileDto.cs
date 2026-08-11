using System.ComponentModel.DataAnnotations;

namespace Labor.Models.DTOs.Auth
{
    public class CompleteProfileDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;
        
        [EmailAddress]
        public string? Email { get; set; }
        
        // Address information for order placement
        [Required]
        public string Street { get; set; } = string.Empty;
        
        [Required]
        public string City { get; set; } = string.Empty;
        
        [Required]
        public string State { get; set; } = string.Empty;
        
        [Required]
        public string Country { get; set; } = "India";
        
        [Required]
        public string ZipCode { get; set; } = string.Empty;
        
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
