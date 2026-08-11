using System.ComponentModel.DataAnnotations;

namespace Labor.Models.DTOs.Labor
{
    public class LaborReviewDto
    {
        public int? ReviewId { get; set; }
        public int? OrderItemId { get; set; }
        
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [StringLength(1000)]
        public string? Comment { get; set; }
        
        public string? EmployerName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 