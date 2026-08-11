using System.ComponentModel.DataAnnotations;

namespace Labor.Models.Entities.Order
{
    public class OrderTracking
    {
        public int Id { get; set; }
        
        public int OrderId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(255)]
        public string? Location { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public Guid? CreatedBy { get; set; }
        
        // Navigation properties
        public virtual Order Order { get; set; } = null!;
    }
} 