using System.ComponentModel.DataAnnotations;

namespace Labor.Models.Entities.User
{
    public class Address
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string AddressType { get; set; } = string.Empty; // 'Home', 'Work', 'Billing', etc.
        
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
        
        public bool IsDefault { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Order.Order> Orders { get; set; } = new List<Order.Order>();
    }
} 