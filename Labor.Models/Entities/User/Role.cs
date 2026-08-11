using System.ComponentModel.DataAnnotations;

namespace Labor.Models.Entities.User
{
    public class Role
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string RoleName { get; set; } = string.Empty;
        
        [StringLength(255)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string? CreatedBy { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public string? UpdatedBy { get; set; }
        
        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<System.RolePermission> RolePermissions { get; set; } = new List<System.RolePermission>();
    }
} 