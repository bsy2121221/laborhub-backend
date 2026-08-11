using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Labor.Models.DTOs.Cart;
using Labor.Models.DTOs.Common;
using System.Security.Claims;
using Labor.Models.DTOs.Order;
using Labor.DataAccess.IServices;

namespace Labor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<CartSummaryDto>>> GetCart([FromQuery] string? sessionId = null)
        {
            try
            {
                string cartIdentifier;
                bool isAuthenticated = User.Identity?.IsAuthenticated == true;

                if (isAuthenticated)
                {
                    cartIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                }
                else if (!string.IsNullOrEmpty(sessionId))
                {
                    cartIdentifier = sessionId;
                }
                else
                {
                    // Return empty cart for anonymous users without session
                    var emptyCart = new CartSummaryDto { Items = new List<CartItemDto>(), TotalAmount = 0, TotalItems = 0 };
                    return Ok(ApiResponse<CartSummaryDto>.SuccessResponse(emptyCart, "Empty cart"));
                }

                var cart = await _cartService.GetCartItemsAsync(cartIdentifier, isAuthenticated);
                
                return Ok(ApiResponse<CartSummaryDto>.SuccessResponse(cart, "Cart retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart");
                return StatusCode(500, ApiResponse<CartSummaryDto>.ErrorResponse("Internal server error"));
            }
        }

        [HttpPost("add")]
        public async Task<ActionResult<ApiResponse<AddToCartResponseDto>>> AddToCart([FromBody] AddToCartDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<AddToCartResponseDto>.ErrorResponse("Validation failed", errors));
                }

                string cartIdentifier;
                bool isAuthenticated = User.Identity?.IsAuthenticated == true;

                if (isAuthenticated)
                {
                    cartIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                }
                else
                {
                    // Generate session ID for anonymous users
                    cartIdentifier = request.SessionId ?? Guid.NewGuid().ToString();
                }

                var cartId = await _cartService.AddToCartAsync(
                    cartIdentifier,
                    isAuthenticated,
                    request.LaborId, 
                    request.RequiredHours, 
                    request.WorkDescription, 
                    request.PreferredDate
                );

                var response = new AddToCartResponseDto 
                { 
                    CartId = cartId, 
                    SessionId = isAuthenticated ? null : cartIdentifier 
                };

                return Ok(ApiResponse<AddToCartResponseDto>.SuccessResponse(response, "Item added to cart successfully"));
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                _logger.LogWarning(ex, "Add to cart failed");
                return BadRequest(ApiResponse<AddToCartResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return StatusCode(500, ApiResponse<AddToCartResponseDto>.ErrorResponse("Internal server error"));
            }
        }

        [HttpPut("{cartId}")]
        public async Task<ActionResult<ApiResponse>> UpdateCartItem(int cartId, [FromBody] UpdateCartItemDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse.ErrorResponse("Validation failed", errors));
                }

                var employerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                var updated = await _cartService.UpdateCartItemAsync(
                    cartId, 
                    employerId,
                    request.laborId,
                    request.RequiredHours, 
                    request.WorkDescription, 
                    request.PreferredDate
                );

                if (!updated)
                {
                    return NotFound(ApiResponse.ErrorResponse("Cart item not found"));
                }

                return Ok(ApiResponse.SuccessResponse("Cart item updated successfully"));
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                _logger.LogWarning(ex, "Update cart item failed");
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }

        [HttpDelete("{cartId}")]
        public async Task<ActionResult<ApiResponse>> RemoveFromCart(int cartId)
        {
            try
            {
                var employerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var removed = await _cartService.RemoveFromCartAsync(cartId, employerId);
                
                if (!removed)
                {
                    return NotFound(ApiResponse.ErrorResponse("Cart item not found"));
                }

                return Ok(ApiResponse.SuccessResponse("Item removed from cart successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }

        [HttpDelete("clear")]
        public async Task<ActionResult<ApiResponse>> ClearCart()
        {
            try
            {
                var employerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var cleared = await _cartService.ClearCartAsync(employerId);
                
                return Ok(ApiResponse.SuccessResponse("Cart cleared successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }

        [HttpGet("count")]
        public async Task<ActionResult<ApiResponse<int>>> GetCartItemCount([FromQuery] string? sessionId = null)
        {
            try
            {
                string cartIdentifier;
                bool isAuthenticated = User.Identity?.IsAuthenticated == true;

                if (isAuthenticated)
                {
                    cartIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                }
                else if (!string.IsNullOrEmpty(sessionId))
                {
                    cartIdentifier = sessionId;
                }
                else
                {
                    return Ok(ApiResponse<int>.SuccessResponse(0, "Empty cart"));
                }

                var count = await _cartService.GetCartItemCountAsync(cartIdentifier, isAuthenticated);
                
                return Ok(ApiResponse<int>.SuccessResponse(count, "Cart item count retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart item count");
                return StatusCode(500, ApiResponse<int>.ErrorResponse("Internal server error"));
            }
        }

        [HttpPost("checkout")]
        [Authorize(Roles = "Employer,Admin")]
        public async Task<ActionResult<ApiResponse<CheckoutResponseDto>>> Checkout([FromBody] CheckoutRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<CheckoutResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var employerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                //// If user has a session cart, merge it with their user cart
                //if (!string.IsNullOrEmpty(request.SessionId))
                //{
                //    await _cartRepository.MergeSessionCartAsync(request.SessionId, employerId.ToString());
                //}

                // Get the cart items for checkout
                var cart = await _cartService.GetCartItemsAsync(employerId.ToString(), true);
                
                if (cart.TotalItems == 0)
                {
                    return BadRequest(ApiResponse<CheckoutResponseDto>.ErrorResponse("Cart is empty"));
                }

                var response = new CheckoutResponseDto 
                { 
                    CartSummary = cart,
                    Message = "Ready for checkout. Please provide address and payment details.",
                    RequireAddress = true,
                    RequirePayment = true
                };

                return Ok(ApiResponse<CheckoutResponseDto>.SuccessResponse(response, "Checkout initiated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout");
                return StatusCode(500, ApiResponse<CheckoutResponseDto>.ErrorResponse("Internal server error"));
            }
        }
    }
} 