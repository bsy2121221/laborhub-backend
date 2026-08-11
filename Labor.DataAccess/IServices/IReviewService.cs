using Labor.Models.DTOs.Labor;

namespace Labor.DataAccess.IServices;

public interface IReviewService
{
    Task<int> AddReviewAsync(int employerId, AddLaborReviewRequestDto request);
    Task<bool> UpdateReviewAsync(int reviewId, int employerId, UpdateLaborReviewRequestDto request);
    Task<bool> DeleteReviewAsync(int reviewId, int employerId);
    Task<IEnumerable<LaborReviewDto>> GetLaborReviewsAsync(int laborId, int pageNumber = 1, int pageSize = 10);
    Task<LaborReviewDto?> GetReviewByIdAsync(int reviewId);
    Task<decimal> GetLaborAverageRatingAsync(int laborId);
}
