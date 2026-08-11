namespace Labor.Models.DTOs.Admin
{
    public class AdminOrderListItemDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int EmployerId { get; set; }
        public string? EmployerName { get; set; }
        public string? EmployerMobile { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime? ScheduledDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}