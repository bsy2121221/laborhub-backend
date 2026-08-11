using System.ComponentModel.DataAnnotations;

namespace Labor.Models.Entities.Order
{
    public class OrderItem
    {
        public int Id { get; set; }
        
        public int OrderId { get; set; }
        
        public int LaborId { get; set; }
        
        [Required]
        public int RequiredHours { get; set; }
        
        [Required]
        public decimal DailyRate { get; set; }
        
        [Required]
        public decimal TotalAmount { get; set; }
        
        [StringLength(1000)]
        public string? WorkDescription { get; set; }
        
        [StringLength(50)]
        public string ItemStatus { get; set; } = "Pending"; // 'Pending', 'Assigned', 'InProgress', 'Completed', 'Cancelled'
        
        public int? ActualHours { get; set; }
        
        public DateTime? StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        // Navigation properties
        public virtual Order Order { get; set; } = null!;
        public virtual Labor.Laborer Labor { get; set; } = null!;
        public virtual ICollection<Labor.LaborReview> LaborReviews { get; set; } = new List<Labor.LaborReview>();
    }
} 