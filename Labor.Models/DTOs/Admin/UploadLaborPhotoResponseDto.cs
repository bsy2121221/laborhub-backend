using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labor.Models.DTOs.Admin
{
    public class UploadLaborPhotoResponseDto
    {
        public string RelativeUrl { get; set; } = string.Empty;
        public string AbsoluteUrl { get; set; } = string.Empty;
    }
}
