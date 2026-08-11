namespace Labor.Models.DTOs.Cart
{
    public class CartSummaryDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
    }
} 