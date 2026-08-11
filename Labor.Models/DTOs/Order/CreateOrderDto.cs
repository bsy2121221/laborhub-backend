using System.ComponentModel.DataAnnotations;

namespace Labor.Models.DTOs.Order
{
    public class CreateOrderDto
    {
        [Required]
        public int WorkAddressId { get; set; }
        
        public DateTime? ScheduledDate { get; set; }
        
        public string? SpecialInstructions { get; set; }
    }
} 