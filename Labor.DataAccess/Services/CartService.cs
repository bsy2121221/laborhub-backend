using Labor.DataAccess.IRepositories;
using Labor.DataAccess.IServices;
using Labor.Models.DTOs.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labor.DataAccess.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;

        public CartService(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }
        public Task<int> AddToCartAsync(string cartIdentifier, bool isAuthenticated, int laborId, int requiredHours, string? workDescription, DateTime? preferredDate)
        {
            return _cartRepository.AddToCartAsync( cartIdentifier,  isAuthenticated,  laborId,  requiredHours, workDescription, preferredDate);
        }

        public Task<bool> ClearCartAsync(int employerId)
        {
            return _cartRepository.ClearCartAsync(employerId);
        }

        public Task<int> GetCartItemCountAsync(string cartIdentifier, bool isAuthenticated)
        {
            return _cartRepository.GetCartItemCountAsync(cartIdentifier, isAuthenticated);
        }

        public Task<CartSummaryDto> GetCartItemsAsync(string cartIdentifier, bool isAuthenticated)
        {
            return _cartRepository.GetCartItemsAsync(cartIdentifier, isAuthenticated);
        }

        public Task<bool> MergeSessionCartAsync(string sessionId, string userId)
        {
            return _cartRepository.MergeSessionCartAsync(sessionId, userId);
        }

        public Task<bool> RemoveFromCartAsync(int cartId, int employerId)
        {
            return _cartRepository.RemoveFromCartAsync(cartId, employerId);
        }

        public Task<bool> UpdateCartItemAsync(int cartId, int employerId, int laborId, int requiredHours, string? workDescription, DateTime? preferredDate)
        {
            return _cartRepository.UpdateCartItemAsync(cartId,employerId, laborId, requiredHours, workDescription, preferredDate);
        }
    }
}
