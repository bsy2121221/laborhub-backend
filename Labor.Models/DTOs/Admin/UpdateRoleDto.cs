using System.ComponentModel.DataAnnotations;


namespace Labor.Models.DTOs.Admin
{
    public class UpdateRoleDto
    {
        [Required]
        [StringLength(100)]
        public string RoleName { get; set; } = string.Empty;
        [StringLength(250)]
        public string Description { get; set; }=string.Empty;
        public bool IsActive { get; set; } = true;
        public int? UpdatedBy { get; set; }

    }
}
