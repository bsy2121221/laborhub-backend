using Labor.Models.DTOs.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labor.DataAccess.IServices
{
    public interface ICartService
    {
        Task<int> AddToCartAsync(string cartIdentifier, bool isAuthenticated, int laborId, int requiredHours, string? workDescription, DateTime? preferredDate);
        Task<CartSummaryDto> GetCartItemsAsync(string cartIdentifier, bool isAuthenticated);
        Task<bool> UpdateCartItemAsync(int cartId, int employerId, int laborId, int requiredHours, string? workDescription, DateTime? preferredDate);
        Task<bool> RemoveFromCartAsync(int cartId, int employerId);
        Task<bool> ClearCartAsync(int employerId);
        Task<int> GetCartItemCountAsync(string cartIdentifier, bool isAuthenticated);
        Task<bool> MergeSessionCartAsync(string sessionId, string userId);
    }
}
