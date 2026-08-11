using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labor.Models.DTOs.Labor
{
    public class UpsertLaborAvailabilityRequestDto
    {
        public List<LaborAvailabilityItemDto> Items { get; set; } = new();
    }
    public class LaborAvailabilityItemDto
    {
        public DateTime AvailableDate { get; set; }
        public string Status { get; set; } = "Available";
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}
