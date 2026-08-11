using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labor.Models.DTOs.Admin
{
    public class CreateUpdateRolePermissionsDto
    {
        [Required]
        [StringLength(100)]
        public string FeatureName { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public int? CreatedBy { get; set; } 
        public int? UpdatedBy { get; set; }
    }
}
