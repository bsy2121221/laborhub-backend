using System.ComponentModel.DataAnnotations;

namespace Labor.Models.Entities.Order
{
    public class Order
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;
        
        public int EmployerId { get; set; }
        
        [Required]
        public decimal TotalAmount { get; set; }
        
        [StringLength(50)]
        public string OrderStatus { get; set; } = "Pending"; // 'Pending', 'Confirmed', 'InProgress', 'Completed', 'Cancelled'
        
        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Pending"; // 'Pending', 'Paid', 'Failed', 'Refunded'
        
        public int WorkAddressId { get; set; }
        
        public DateTime? ScheduledDate { get; set; }
        
        public DateTime? CompletedDate { get; set; }
        
        public DateTime? CancelledDate { get; set; }

        public bool Is_deleted { get; set; } = false;

        [StringLength(500)]
        public string? CancelReason { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual User.User Employer { get; set; } = null!;
        public virtual User.Address WorkAddress { get; set; } = null!;
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<OrderTracking> OrderTrackings { get; set; } = new List<OrderTracking>();
    }
} 