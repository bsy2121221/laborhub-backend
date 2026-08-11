namespace Labor.Models.DTOs.Order
{
    public class OrderTrackingDto
    {
        public int TrackingId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 