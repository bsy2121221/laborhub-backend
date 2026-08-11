using System.ComponentModel.DataAnnotations;

namespace Labor.Models.Entities.Labor
{
    public class LaborSkill
    {
        public int Id { get; set; }
        
        public int LaborId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string SkillName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string ProficiencyLevel { get; set; } = string.Empty; // 'Beginner', 'Intermediate', 'Advanced', 'Expert'
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual Laborer Labor { get; set; } = null!;
    }
} 