using Labor.Models.DTOs.Cart;
using Labor.Models.Entities.Order;

namespace Labor.DataAccess.IRepositories
{
    public interface ICartRepository
    {
        Task<int> AddToCartAsync(string cartIdentifier, bool isAuthenticated, int laborId, int requiredHours, string? workDescription, DateTime? preferredDate);
        Task<CartSummaryDto> GetCartItemsAsync(string cartIdentifier, bool isAuthenticated);
        Task<bool> UpdateCartItemAsync(int cartId, int employerId,int laborId, int requiredHours, string? workDescription, DateTime? preferredDate);
        Task<bool> RemoveFromCartAsync(int cartId, int employerId);
        Task<bool> ClearCartAsync(int employerId);
        Task<int> GetCartItemCountAsync(string cartIdentifier, bool isAuthenticated);
        Task<bool> MergeSessionCartAsync(string sessionId, string userId);
    }
} 