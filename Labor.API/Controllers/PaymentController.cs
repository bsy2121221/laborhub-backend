using Labor.Models.DTOs.Common;
using Labor.Models.DTOs.Payment;
using Labor.DataAccess.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace Labor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Employer,Admin")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpGet("orders/{orderId:int}/summary")]
    public async Task<ActionResult<ApiResponse<PaymentSummaryDto>>> GetSummary(int orderId)
    {
        try
        {
            var employerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var summary = await _paymentService.GetSummaryAsync(orderId, employerId);
            return Ok(ApiResponse<PaymentSummaryDto>.SuccessResponse(summary));
        }
        catch (SqlException ex)
        {
            _logger.LogWarning(ex, "Payment summary failed for order {OrderId}", orderId);
            return NotFound(ApiResponse<PaymentSummaryDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PaymentSummaryDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("orders/{orderId:int}/create")]
    public async Task<ActionResult<ApiResponse<CreatePaymentResponseDto>>> CreatePayment(
        int orderId,
        [FromBody] CreatePaymentRequestDto? request)
    {
        try
        {
            var employerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _paymentService.CreatePaymentAsync(orderId, employerId, request);
            return Ok(ApiResponse<CreatePaymentResponseDto>.SuccessResponse(result, "Payment session created."));
        }
        catch (SqlException ex)
        {
            _logger.LogWarning(ex, "Create payment failed for order {OrderId}", orderId);
            return BadRequest(ApiResponse<CreatePaymentResponseDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CreatePaymentResponseDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("verify")]
    public async Task<ActionResult<ApiResponse<PaymentCompleteResultDto>>> VerifyPayment([FromBody] VerifyPaymentRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PaymentCompleteResultDto>.ErrorResponse("Validation failed", errors));
        }

        try
        {
            var employerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _paymentService.VerifyAndCompleteAsync(employerId, request);
            return Ok(ApiResponse<PaymentCompleteResultDto>.SuccessResponse(result, "Payment successful."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PaymentCompleteResultDto>.ErrorResponse(ex.Message));
        }
    }
}
