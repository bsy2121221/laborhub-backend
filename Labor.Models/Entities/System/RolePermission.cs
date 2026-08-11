using System.ComponentModel.DataAnnotations;

namespace Labor.Models.Entities.System
{
    public class RolePermission
    {
        public int Id { get; set; }
        
        public int RoleId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FeatureName { get; set; } = string.Empty;
        
        public bool CanView { get; set; } = false;
        
        public bool CanCreate { get; set; } = false;
        
        public bool CanEdit { get; set; } = false;
        
        public bool CanDelete { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string? CreatedBy { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public string? UpdatedBy { get; set; }
        
        // Navigation properties
        public virtual User.Role Role { get; set; } = null!;
    }
} 