using System.ComponentModel.DataAnnotations;

namespace Labor.Models.DTOs.Admin
{
    public class CreateRolePermissionDto
    {
        [Required]
        public int RoleId { get; set; }
        [Required]
        [StringLength(250)]
        public string FeatureName { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
        public bool CanCreate { get; set; }
        public bool CanDelete { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }

    }
}
