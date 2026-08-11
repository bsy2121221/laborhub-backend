using Dapper;
using Labor.DataAccess.Context;
using Labor.DataAccess.IRepositories;
using Labor.Models.DTOs.Order;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labor.DataAccess.Repositories
{
    public class LaborConfirmationRepository : ILaborConfirmationRepository
    {
        private readonly IDbContext _dbContext;

        public LaborConfirmationRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public Task EnqueueForOrderAsync(int orderId)
        {
            return Task.CompletedTask;
        }

        public async Task<(int Total, int Confirmed, int Pending, int Declined)> GetOrderCountsAsync(int orderId)
        {
            using var conn = _dbContext.CreateConnection();
            var row = await conn.QuerySingleAsync<dynamic>(@"
            SELECT
              COUNT(*) Total,
              SUM(CASE WHEN ItemStatus='Assigned' THEN 1 ELSE 0 END) Confirmed,
              SUM(CASE WHEN ItemStatus='PendingConfirmation' THEN 1 ELSE 0 END) Pending,
              SUM(CASE WHEN ItemStatus IN ('Declined','Cancelled') THEN 1 ELSE 0 END) Declined
            FROM OrderItems WHERE OrderID=@OrderID", new { OrderID = orderId });
            return ((int)row.Total, (int)row.Confirmed, (int)row.Pending, (int)row.Declined);
        }
        

        public async Task<OrderLaborSummaryDto?> GetOrderLaborSummaryAsync(int orderId, int? userId)
        {
            using var conn = _dbContext.CreateConnection();
            using var multi = await conn.QueryMultipleAsync(
                "[dbo].[sp_GetOrderLaborSummary]",
                new { OrderID = orderId, UserID = userId },
                commandType: CommandType.StoredProcedure);
            var header = await multi.ReadFirstOrDefaultAsync<OrderSummaryHeaderDto>();
            if (header == null) return null;
            var items = (await multi.ReadAsync<OrderLaborItemDto>()).ToList();

            // Always merge reviews so ratings show even if SP join columns are missing on older DBs
            var reviewRows = await conn.QueryAsync<OrderItemReviewRow>(@"
                SELECT
                    lr.OrderItemID AS OrderItemId,
                    lr.ID AS ReviewId,
                    lr.Rating AS ReviewRating,
                    lr.Comment AS ReviewComment
                FROM [dbo].[LaborReviews] lr
                INNER JOIN [dbo].[OrderItems] oi ON lr.OrderItemID = oi.ID
                WHERE oi.OrderID = @OrderID AND lr.IsActive = 1",
                new { OrderID = orderId });

            var reviewByItem = reviewRows.ToDictionary(r => r.OrderItemId);
            foreach (var item in items)
            {
                if (reviewByItem.TryGetValue(item.OrderItemId, out var review))
                {
                    item.HasReview = true;
                    item.ReviewId = review.ReviewId;
                    item.ReviewRating = review.ReviewRating;
                    item.ReviewComment = review.ReviewComment;
                }
            }

            return new OrderLaborSummaryDto { Header = header, LaborItems = items };
        }

        private sealed class OrderItemReviewRow
        {
            public int OrderItemId { get; set; }
            public int ReviewId { get; set; }
            public int ReviewRating { get; set; }
            public string? ReviewComment { get; set; }
        }

        public async Task<IEnumerable<dynamic>> GetPendingForCallAsync()
        {
            using var conn = _dbContext.CreateConnection();
            return await conn.QueryAsync<dynamic>(
                "[dbo].[sp_GetPendingLaborConfirmationsForCall]",
                commandType: CommandType.StoredProcedure);
        }

        public async Task MarkCallAttemptAsync(int laborConfirmationId, string? providerCallId)
        {
            using var conn = _dbContext.CreateConnection();
            await conn.ExecuteAsync(
                "[dbo].[sp_MarkLaborConfirmationCallAttempt]",
                new { LaborConfirmationID = laborConfirmationId, ProviderCallId = providerCallId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<LaborIvrProcessResult> ProcessIvrDigitAsync(int laborConfirmationId, string digit, int? updatedBy)
        {
            using var conn = _dbContext.CreateConnection();
            var result = await conn.QuerySingleAsync<LaborIvrProcessResult>(
                "[dbo].[sp_ProcessLaborIvrResponse]",
                new { LaborConfirmationID = laborConfirmationId, Digit = digit, UpdatedBy = updatedBy },
                commandType: CommandType.StoredProcedure);
            return result;
        }

        public async Task<int?> GetPendingLaborConfirmationIdAsync(int orderItemId, int laborUserId)
        {
            using var conn = _dbContext.CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<int?>(@"
                SELECT lc.ID
                FROM [dbo].[LaborConfirmations] lc
                INNER JOIN [dbo].[OrderItems] oi ON lc.OrderItemID = oi.ID
                INNER JOIN [dbo].[Labors] l ON oi.LaborID = l.ID
                WHERE oi.ID = @OrderItemID
                  AND l.UserID = @LaborUserID
                  AND oi.ItemStatus = N'PendingConfirmation'
                  AND lc.Status = N'Pending'",
                new { OrderItemID = orderItemId, LaborUserID = laborUserId });
        }
    }
}
