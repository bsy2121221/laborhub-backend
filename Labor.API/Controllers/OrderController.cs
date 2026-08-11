using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Labor.Models.DTOs.Order;
using Labor.Models.DTOs.Common;
using System.Security.Claims;
using Labor.DataAccess.IRepositories;
using Microsoft.Data.SqlClient;
using Labor.DataAccess.IServices;

namespace Labor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        
        private readonly IOrderService _orderService;
        private readonly IUserRepository _userRepository;
        private readonly ILaborConfirmationService _laborConfirmationService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            IUserRepository userRepository,
            ILaborConfirmationService laborConfirmationService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _userRepository = userRepository;
            _laborConfirmationService = laborConfirmationService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Employer,Admin")]
        public async Task<ActionResult<ApiResponse<OrderCreationResponseDto>>> CreateOrder([FromBody] CreateOrderDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<OrderCreationResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var employerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                // Employers must complete profile; admins can test the full booking flow without it
                if (userRole != "Admin")
                {
                    var isProfileComplete = await _userRepository.IsProfileCompleteAsync(employerId);
                    if (!isProfileComplete)
                    {
                        return BadRequest(ApiResponse<OrderCreationResponseDto>.ErrorResponse("Please complete your profile before placing an order", new List<string> { "PROFILE_INCOMPLETE" }));
                    }
                }

                var (orderId, orderNumber) = await _orderService.CreateOrderFromCartAsync(
                    employerId, 
                    request.WorkAddressId, 
                    request.ScheduledDate
                );

                var response = new OrderCreationResponseDto
                {
                    OrderId = orderId,
                    OrderNumber = orderNumber
                };

                await _laborConfirmationService.EnqueueConfirmationsForOrderAsync(orderId);

                return Ok(ApiResponse<OrderCreationResponseDto>.SuccessResponse(
                    response,
                    "Order placed. We are calling labor for confirmation."));
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "vlidation creating order");
                return BadRequest(ApiResponse<OrderCreationResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, ApiResponse<OrderCreationResponseDto>.ErrorResponse("Internal server error"));
            }
        }

        [HttpGet("{orderId}")]
        public async Task<ActionResult<ApiResponse<OrderResponseDto>>> GetOrderDetails(int orderId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                
                // Admin can see all orders, others can only see their own
                int? filterUserId = userRole == "Admin" ? null : userId;
                
                var order = await _orderService.GetOrderDetailsAsync(orderId, filterUserId);
                if (order == null)
                {
                    return NotFound(ApiResponse<OrderResponseDto>.ErrorResponse("Order not found"));
                }

                return Ok(ApiResponse<OrderResponseDto>.SuccessResponse(order, "Order details retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order details for {OrderId}", orderId);
                return StatusCode(500, ApiResponse<OrderResponseDto>.ErrorResponse("Internal server error"));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<dynamic>>>> GetMyOrders(
            [FromQuery] string? orderStatus = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                IEnumerable<dynamic> orders;

                if (userRole == "Labor")
                {
                    orders = await _orderService.GetLaborOrdersAsync(userId, orderStatus, pageNumber, pageSize);
                    return Ok(ApiResponse<IEnumerable<dynamic>>.SuccessResponse(orders, "Work assignments retrieved successfully"));
                }

                orders = await _orderService.GetUserOrdersAsync(userId, orderStatus, pageNumber, pageSize);
                return Ok(ApiResponse<IEnumerable<dynamic>>.SuccessResponse(orders, "Orders retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user orders");
                return StatusCode(500, ApiResponse<IEnumerable<dynamic>>.ErrorResponse("Internal server error"));
            }
        }

        [HttpGet("{orderId}/labor-summary")]
        public async Task<ActionResult<ApiResponse<OrderLaborSummaryDto>>> GetOrderLaborSummary(int orderId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                int? filterUserId = userRole == "Admin" ? null : userId;

                var summary = await _laborConfirmationService.GetOrderLaborSummaryAsync(orderId, filterUserId);
                if (summary == null)
                {
                    return NotFound(ApiResponse<OrderLaborSummaryDto>.ErrorResponse("Order not found"));
                }

                return Ok(ApiResponse<OrderLaborSummaryDto>.SuccessResponse(summary, "Order labor summary retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order labor summary for {OrderId}", orderId);
                return StatusCode(500, ApiResponse<OrderLaborSummaryDto>.ErrorResponse("Internal server error"));
            }
        }

        [HttpPut("{orderId}/status")]
        [Authorize(Roles = "Admin,Labor")]
        public async Task<ActionResult<ApiResponse>> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse.ErrorResponse("Validation failed", errors));
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                var updated = await _orderService.UpdateOrderStatusAsync(
                    orderId, 
                    request.NewStatus, 
                    request.Description, 
                    userId
                );

                if (!updated)
                {
                    return NotFound(ApiResponse.ErrorResponse("Order not found"));
                }

                return Ok(ApiResponse.SuccessResponse("Order status updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }

        [HttpPut("items/{orderItemId}/status")]
        [Authorize(Roles = "Labor,Admin")]
        public async Task<ActionResult<ApiResponse>> UpdateOrderItemStatus(int orderItemId, [FromBody] UpdateOrderItemStatusDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse.ErrorResponse("Validation failed", errors));
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var updated = await _orderService.UpdateOrderItemStatusAsync(
                    orderItemId,
                    request.ItemStatus,
                    request.ActualHours,
                    request.StartTime,
                    request.EndTime,
                    userId);

                if (!updated)
                {
                    return NotFound(ApiResponse.ErrorResponse("Order item not found or status change not allowed"));
                }

                return Ok(ApiResponse.SuccessResponse("Order item status updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order item status");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }

        [HttpPost("items/{orderItemId}/confirm")]
        [Authorize(Roles = "Labor,Admin")]
        public async Task<ActionResult<ApiResponse>> ConfirmAvailability(int orderItemId, [FromBody] LaborConfirmAvailabilityDto request)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _laborConfirmationService.ProcessAppConfirmationAsync(
                    orderItemId,
                    userId,
                    request.Accepted,
                    userId);

                if (!result.Success)
                {
                    return BadRequest(ApiResponse.ErrorResponse(
                        request.Accepted
                            ? "Could not confirm. This job may already be responded to."
                            : "Could not decline. This job may already be responded to."));
                }

                return Ok(ApiResponse.SuccessResponse(
                    request.Accepted
                        ? "You confirmed you will come for this job."
                        : "You declined this job. Your calendar slot has been freed."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming labor availability for item {OrderItemId}", orderItemId);
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }

        [HttpPost("{orderId}/tracking")]
        [Authorize(Roles = "Admin,Labor")]
        public async Task<ActionResult<ApiResponse>> AddOrderTracking(int orderId, [FromBody] AddOrderTrackingDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse.ErrorResponse("Validation failed", errors));
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                var added = await _orderService.AddOrderTrackingAsync(
                    orderId, 
                    request.Status, 
                    request.Description, 
                    request.Location, 
                    userId
                );

                if (!added)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Failed to add tracking information"));
                }

                return Ok(ApiResponse.SuccessResponse("Tracking information added successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding order tracking");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }
    }

} 