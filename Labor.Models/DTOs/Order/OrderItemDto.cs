namespace Labor.Models.DTOs.Order
{
    public class OrderItemDto
    {
        public int OrderItemId { get; set; }
        public int LaborId { get; set; }
        public int RequiredHours { get; set; }
        public decimal DailyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? WorkDescription { get; set; }
        public string ItemStatus { get; set; } = string.Empty;
        public int? ActualHours { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string LaborName { get; set; } = string.Empty;
        public string LaborMobile { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public string LaborType { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public decimal Rating { get; set; }
    }

    public class UpdateCartItemDto
    {
        public int laborId { get; set; }
        public int RequiredHours { get; set; }
        public string? WorkDescription { get; set; }
        public DateTime? PreferredDate { get; set; }
    }

    public class CheckoutRequestDto
    {
        public string? SessionId { get; set; }
        public int? AddressId { get; set; }
    }
} 