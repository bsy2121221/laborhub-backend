using Labor.DataAccess.IServices;
using Labor.Models.DTOs.Common;
using Labor.Models.DTOs.Labor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace Labor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly ILogger<ReviewController> _logger;

    public ReviewController(IReviewService reviewService, ILogger<ReviewController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "Employer,Admin")]
    public async Task<ActionResult<ApiResponse<AddLaborReviewResponseDto>>> AddReview([FromBody] AddLaborReviewRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<AddLaborReviewResponseDto>.ErrorResponse("Validation failed", errors));
        }

        try
        {
            var employerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var reviewId = await _reviewService.AddReviewAsync(employerId, request);
            return Ok(ApiResponse<AddLaborReviewResponseDto>.SuccessResponse(
                new AddLaborReviewResponseDto { ReviewId = reviewId },
                "Thank you for your feedback."));
        }
        catch (SqlException ex)
        {
            _logger.LogWarning(ex, "Add review failed for order item {OrderItemId}", request.OrderItemId);
            return BadRequest(ApiResponse<AddLaborReviewResponseDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("labor/{laborId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IEnumerable<LaborReviewDto>>>> GetLaborReviews(
        int laborId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var reviews = await _reviewService.GetLaborReviewsAsync(laborId, pageNumber, pageSize);
        return Ok(ApiResponse<IEnumerable<LaborReviewDto>>.SuccessResponse(reviews));
    }

    [HttpGet("labor/{laborId:int}/average-rating")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<decimal>>> GetAverageRating(int laborId)
    {
        var avg = await _reviewService.GetLaborAverageRatingAsync(laborId);
        return Ok(ApiResponse<decimal>.SuccessResponse(avg));
    }

    [HttpGet("{reviewId:int}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<LaborReviewDto>>> GetReview(int reviewId)
    {
        var review = await _reviewService.GetReviewByIdAsync(reviewId);
        if (review == null)
        {
            return NotFound(ApiResponse<LaborReviewDto>.ErrorResponse("Review not found"));
        }

        return Ok(ApiResponse<LaborReviewDto>.SuccessResponse(review));
    }

    [HttpPut("{reviewId:int}")]
    [Authorize(Roles = "Employer,Admin")]
    public async Task<ActionResult<ApiResponse>> UpdateReview(int reviewId, [FromBody] UpdateLaborReviewRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse.ErrorResponse("Validation failed", errors));
        }

        var employerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var updated = await _reviewService.UpdateReviewAsync(reviewId, employerId, request);
        if (!updated)
        {
            return NotFound(ApiResponse.ErrorResponse("Review not found"));
        }

        return Ok(ApiResponse.SuccessResponse("Review updated successfully"));
    }

    [HttpDelete("{reviewId:int}")]
    [Authorize(Roles = "Employer,Admin")]
    public async Task<ActionResult<ApiResponse>> DeleteReview(int reviewId)
    {
        var employerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var deleted = await _reviewService.DeleteReviewAsync(reviewId, employerId);
        if (!deleted)
        {
            return NotFound(ApiResponse.ErrorResponse("Review not found"));
        }

        return Ok(ApiResponse.SuccessResponse("Review removed successfully"));
    }
}
