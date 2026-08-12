using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labor.Models.DTOs.Admin
{
    public class AdminCreateUpdateLaborTypes
    {
        public string TypeName { get; set; } = string.Empty;
        public string Description { get; set; }=string.Empty;
        public double DailyRate { get; set; }
        public bool isActive { get; set; }
    }
}
