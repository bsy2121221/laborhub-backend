using System.ComponentModel.DataAnnotations;

namespace Labor.Models.Entities.Labor
{
    public class LaborType
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string TypeName { get; set; } = string.Empty;
        
        [StringLength(255)]
        public string? Description { get; set; }
        
        public decimal DailyRate { get; set; } = 0;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string? CreatedBy { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public string? UpdatedBy { get; set; }
        
        // Navigation properties
        public virtual ICollection<Laborer> Labors { get; set; } = new List<Laborer>();
    }
} 