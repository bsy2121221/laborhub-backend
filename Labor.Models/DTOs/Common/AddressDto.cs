using System.ComponentModel.DataAnnotations;

namespace Labor.Models.DTOs.Order
{
    public class AddressDto
    {
        public int? AddressId { get; set; }
        
        [StringLength(50)]
        public string AddressType { get; set; } = string.Empty;
        
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
        public bool IsDefault { get; set; }
    }
} 