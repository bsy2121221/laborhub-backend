using Labor.Models.DTOs.Cart;

namespace Labor.Models.DTOs.Order
{
    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int EmployerId { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime? ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string? CancelReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public string EmployerName { get; set; } = string.Empty;
        public string EmployerMobile { get; set; } = string.Empty;
        public AddressDto WorkAddress { get; set; } = new();
        public List<OrderItemDto> OrderItems { get; set; } = new();
        public List<OrderTrackingDto> OrderTrackings { get; set; } = new();
    }

    public class CreateOrderFromCartResult
    {
        public int OrderID { get; set; }      
        public string OrderNumber { get; set; }=string.Empty;
    }

    public class AddToCartResponseDto
    {
        public int CartId { get; set; }
        public string? SessionId { get; set; }
    }

    public class CheckoutResponseDto
    {
        public CartSummaryDto CartSummary { get; set; } = new();
        public string Message { get; set; } = string.Empty;
        public bool RequireAddress { get; set; }
        public bool RequirePayment { get; set; }
    }

    public class OrderCreationResponseDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
    }

    public class UpdateOrderStatusDto
    {
        public string NewStatus { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateOrderItemStatusDto
    {
        public string ItemStatus { get; set; } = string.Empty;
        public int? ActualHours { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class LaborConfirmAvailabilityDto
    {
        public bool Accepted { get; set; }
    }

    public class AddOrderTrackingDto
    {
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
    }
} 