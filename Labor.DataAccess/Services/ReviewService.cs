using Labor.DataAccess.IRepositories;
using Labor.DataAccess.IServices;
using Labor.Models.DTOs.Labor;

namespace Labor.DataAccess.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewService(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public Task<int> AddReviewAsync(int employerId, AddLaborReviewRequestDto request)
    {
        return _reviewRepository.AddLaborReviewAsync(
            request.OrderItemId,
            employerId,
            request.LaborId,
            request.Rating,
            request.Comment);
    }

    public Task<bool> UpdateReviewAsync(int reviewId, int employerId, UpdateLaborReviewRequestDto request)
    {
        return _reviewRepository.UpdateReviewAsync(reviewId, employerId, request.Rating, request.Comment);
    }

    public Task<bool> DeleteReviewAsync(int reviewId, int employerId)
    {
        return _reviewRepository.DeleteReviewAsync(reviewId, employerId);
    }

    public Task<IEnumerable<LaborReviewDto>> GetLaborReviewsAsync(int laborId, int pageNumber = 1, int pageSize = 10)
    {
        return _reviewRepository.GetLaborReviewsAsync(laborId, pageNumber, pageSize);
    }

    public Task<LaborReviewDto?> GetReviewByIdAsync(int reviewId)
    {
        return _reviewRepository.GetReviewByIdAsync(reviewId);
    }

    public Task<decimal> GetLaborAverageRatingAsync(int laborId)
    {
        return _reviewRepository.GetLaborAverageRatingAsync(laborId);
    }
}
