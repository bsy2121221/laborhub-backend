using System.ComponentModel.DataAnnotations;

namespace Labor.Models.Entities.Order
{
    public class Cart
    {
        public int Id { get; set; }
        
        public int EmployerId { get; set; }
        
        public int LaborId { get; set; }
        
        [Required]
        public int RequiredHours { get; set; }
        
        [Required]
        public decimal DailyRate { get; set; }
        
        [Required]
        public decimal TotalAmount { get; set; }
        
        [StringLength(1000)]
        public string? WorkDescription { get; set; }
        
        public DateTime? PreferredDate { get; set; }
        public bool Is_deleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual User.User Employer { get; set; } = null!;
        public virtual Labor.Laborer Labor { get; set; } = null!;
    }
} 