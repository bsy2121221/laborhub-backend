using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labor.Models.DTOs.Order
{
    public class OrderLaborSummaryDto
    {
        public OrderSummaryHeaderDto Header { get; set; } = new();
        public List<OrderLaborItemDto> LaborItems { get; set; } = new();
    }
    public class OrderSummaryHeaderDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = "";
        public string OrderStatus { get; set; } = "";
        public string PaymentStatus { get; set; } = "";
        public DateTime? ScheduledDate { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Street { get; set; } = "";
        public string City { get; set; } = "";
        public string State { get; set; } = "";
        public string ZipCode { get; set; } = "";
        public int TotalLabor { get; set; }
        public int ConfirmedLabor { get; set; }
        public int PendingLabor { get; set; }
        public int DeclinedLabor { get; set; }
    }
    public class OrderLaborItemDto
    {
        public int OrderItemId { get; set; }
        public int LaborId { get; set; }
        public string LaborName { get; set; } = "";
        public string? LaborMobile { get; set; }
        public string LaborType { get; set; } = "";
        public int RequiredHours { get; set; }
        public decimal DailyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? WorkDescription { get; set; }
        public string ItemStatus { get; set; } = "";
        public DateTime? PreferredWorkDate { get; set; }
        public string? ConfirmationStatus { get; set; }
        public int? AttemptCount { get; set; }
        public DateTime? RespondedAt { get; set; }
        public bool HasReview { get; set; }
        public int? ReviewId { get; set; }
        public int? ReviewRating { get; set; }
        public string? ReviewComment { get; set; }
    }
}
