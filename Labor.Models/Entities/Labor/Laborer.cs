using System.ComponentModel.DataAnnotations;

namespace Labor.Models.Entities.Labor
{
    public class Laborer
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public int LaborTypeId { get; set; }
        
        [StringLength(255)]
        public string? Specialization { get; set; }
        
        public int ExperienceYears { get; set; } = 0;
        
        public decimal Rating { get; set; } = 0.00m;
        
        public int TotalReviews { get; set; } = 0;
        
        [Required]
        public decimal DailyRate { get; set; }
        
        public int MinimumHours { get; set; } = 1;
        
        public int MaximumHours { get; set; } = 24;
        
        [StringLength(50)]
        public string AvailabilityStatus { get; set; } = "Available"; // 'Available', 'Busy', 'Unavailable'
        
        public bool IsVerified { get; set; } = false;
        
        public bool IsActive { get; set; } = true;

        public bool Is_deleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string? CreatedBy { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public string? UpdatedBy { get; set; }
        
        // Navigation properties
        public virtual User.User User { get; set; } = null!;
        public virtual LaborType LaborType { get; set; } = null!;
        public virtual ICollection<LaborSkill> LaborSkills { get; set; } = new List<LaborSkill>();
        public virtual ICollection<Order.Cart> Carts { get; set; } = new List<Order.Cart>();
        public virtual ICollection<Order.OrderItem> OrderItems { get; set; } = new List<Order.OrderItem>();
        public virtual ICollection<LaborReview> LaborReviews { get; set; } = new List<LaborReview>();
    }
} 