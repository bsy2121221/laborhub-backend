using System.ComponentModel.DataAnnotations;


namespace Labor.Models.DTOs.Admin
{
    public class CreateRolesDto
    {
        [Required]
        [StringLength(100)]
        public string RoleName { get; set; }=string.Empty;
        [StringLength(250)]
        public string Description { get; set; }=string.Empty;
        public int? CreatedBy { get; set; }
    }
}
