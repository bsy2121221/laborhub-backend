using System.ComponentModel.DataAnnotations;

namespace Labor.Models.Entities.User
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(15)]
        public string MobileNumber { get; set; } = string.Empty;
        
        [StringLength(255)]
        public string? PasswordHash { get; set; } = string.Empty;

        public int? PersonId { get; set; }

        public int RoleId { get; set; }

        public bool IsTemporaryPassword { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        
        public bool IsMobileVerified { get; set; } = false;
        
        public bool IsEmailVerified { get; set; } = false;

        public bool IsProfileComplete { get; set; } = false;

        public bool Is_deleted { get; set; } = false;

        public DateTime? LastLoginAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string? CreatedBy { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public string? UpdatedBy { get; set; }
        
        // Navigation properties
        public virtual Person? Person { get; set; }
        public virtual Role Role { get; set; } = null!;
        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
        public virtual Labor.Laborer? LaborProfile { get; set; }
        public virtual ICollection<Order.Order> Orders { get; set; } = new List<Order.Order>();
        public virtual ICollection<Order.Cart> Carts { get; set; } = new List<Order.Cart>();
        public virtual ICollection<Labor.LaborReview> LaborReviews { get; set; } = new List<Labor.LaborReview>();
    }
} 