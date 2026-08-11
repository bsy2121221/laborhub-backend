using System.ComponentModel.DataAnnotations;

namespace Labor.Models.DTOs.Labor
{
    public class LaborSkillDto
    {
        public int? SkillId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string SkillName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string ProficiencyLevel { get; set; } = string.Empty; // 'Beginner', 'Intermediate', 'Advanced', 'Expert'
    }
} 