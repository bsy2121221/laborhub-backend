using System.ComponentModel.DataAnnotations;

namespace Labor.Models.DTOs.Cart
{
    public class AddToCartDto
    {
        [Required]
        public int LaborId { get; set; }
        
        [Required]
        [Range(1, 24, ErrorMessage = "Required hours must be between 1 and 24")]
        public int RequiredHours { get; set; }
        
        [StringLength(1000)]
        public string? WorkDescription { get; set; }
        
        public DateTime? PreferredDate { get; set; }
        
        // For anonymous users - session identifier
        public string? SessionId { get; set; }
    }
} 