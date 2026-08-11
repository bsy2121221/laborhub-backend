using Labor.Models.DTOs.Labor;
using Labor.Models.Entities.Labor;

namespace Labor.DataAccess.IRepositories
{
    public interface IReviewRepository
    {
        Task<int> AddLaborReviewAsync(int orderItemId, int employerId, int laborId, int rating, string? comment);
        Task<IEnumerable<LaborReviewDto>> GetLaborReviewsAsync(int laborId, int pageNumber = 1, int pageSize = 10);
        Task<LaborReviewDto?> GetReviewByIdAsync(int reviewId);
        Task<bool> UpdateReviewAsync(int reviewId, int employerId, int rating, string? comment);
        Task<bool> DeleteReviewAsync(int reviewId, int employerId);
        Task<bool> HasUserReviewedOrderItemAsync(int orderItemId, int employerId);
        Task<decimal> GetLaborAverageRatingAsync(int laborId);
    }
} 