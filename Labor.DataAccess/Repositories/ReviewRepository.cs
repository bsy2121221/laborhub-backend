using Dapper;
using Labor.DataAccess.Context;
using Labor.DataAccess.IRepositories;
using Labor.Models.DTOs.Labor;
using System.Data;

namespace Labor.DataAccess.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly IDbContext _context;

        public ReviewRepository(IDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddLaborReviewAsync(int orderItemId, int employerId, int laborId, int rating, string? comment)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@OrderItemId", orderItemId);
            parameters.Add("@EmployerId", employerId);
            parameters.Add("@LaborId", laborId);
            parameters.Add("@Rating", rating);
            parameters.Add("@Comment", comment);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_AddLaborReview]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (result is IDictionary<string, object> dict)
            {
                if (dict.TryGetValue("ReviewId", out var id) || dict.TryGetValue("ReviewID", out id))
                {
                    return Convert.ToInt32(id);
                }
            }

            return (int)result.ReviewId;
        }

        public async Task<IEnumerable<LaborReviewDto>> GetLaborReviewsAsync(int laborId, int pageNumber = 1, int pageSize = 10)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@LaborId", laborId);
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);

            var results = await connection.QueryAsync<LaborReviewDto>(
                "[dbo].[sp_GetLaborReviews]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return results;
        }

        public async Task<LaborReviewDto?> GetReviewByIdAsync(int reviewId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ReviewId", reviewId);

            var result = await connection.QueryFirstOrDefaultAsync<LaborReviewDto>(
                "[dbo].[sp_GetReviewById]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<bool> UpdateReviewAsync(int reviewId, int employerId, int rating, string? comment)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ReviewId", reviewId);
            parameters.Add("@EmployerId", employerId);
            parameters.Add("@Rating", rating);
            parameters.Add("@Comment", comment);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_UpdateReview]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.RowsAffected > 0;
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, int employerId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ReviewId", reviewId);
            parameters.Add("@EmployerId", employerId);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_DeleteReview]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.RowsAffected > 0;
        }

        public async Task<bool> HasUserReviewedOrderItemAsync(int orderItemId, int employerId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@OrderItemId", orderItemId);
            parameters.Add("@EmployerId", employerId);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_HasUserReviewedOrderItem]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ReviewCount > 0;
        }

        public async Task<decimal> GetLaborAverageRatingAsync(int laborId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@LaborId", laborId);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_GetLaborAverageRating]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.AverageRating;
        }
    }
} 