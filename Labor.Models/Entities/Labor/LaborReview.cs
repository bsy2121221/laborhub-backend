using System.ComponentModel.DataAnnotations;

namespace Labor.Models.Entities.Labor
{
    public class LaborReview
    {
        public int Id { get; set; }
        
        public int OrderItemId { get; set; }
        
        public int EmployerId { get; set; }
        
        public int LaborId { get; set; }
        
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [StringLength(1000)]
        public string? Comment { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual Order.OrderItem OrderItem { get; set; } = null!;
        public virtual User.User Employer { get; set; } = null!;
        public virtual Laborer Labor { get; set; } = null!;
    }
} 