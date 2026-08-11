namespace Labor.Models.DTOs.Cart
{
    public class CartItemDto
    {
        public int CartId { get; set; }
        public int LaborId { get; set; }
        public int RequiredHours { get; set; }
        public decimal DailyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? WorkDescription { get; set; }
        public DateTime? PreferredDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string LaborName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public string LaborType { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public decimal Rating { get; set; }
        public int TotalReviews { get; set; }
        public bool IsExpired { get; set; }
        public bool IsUnavailableNow { get; set; }
    }
} 