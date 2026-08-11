using Dapper;
using Labor.DataAccess.Context;
using Labor.DataAccess.IRepositories;
using Labor.Models.DTOs.Cart;
using System.Data;

namespace Labor.DataAccess.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly IDbContext _context;

        public CartRepository(IDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddToCartAsync(string cartIdentifier, bool isAuthenticated, int laborId, int requiredHours, string? workDescription, DateTime? preferredDate)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            
            if (isAuthenticated)
            {
                parameters.Add("@EmployerId", int.Parse(cartIdentifier));
                parameters.Add("@LaborId", laborId);
                parameters.Add("@RequiredHours", requiredHours);
                parameters.Add("@WorkDescription", workDescription);
                parameters.Add("@PreferredDate", preferredDate);

                var result = await connection.QuerySingleAsync<dynamic>(
                    "[dbo].[sp_AddToCart]",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.CartId;
            }
            else
            {
                parameters.Add("@SessionId", cartIdentifier);
                parameters.Add("@LaborId", laborId);
                parameters.Add("@RequiredHours", requiredHours);
                parameters.Add("@WorkDescription", workDescription);
                parameters.Add("@PreferredDate", preferredDate);

                var result = await connection.QuerySingleAsync<dynamic>(
                    "[dbo].[sp_AddToSessionCart]",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.CartId;
            }
        }

        public async Task<CartSummaryDto> GetCartItemsAsync(string cartIdentifier, bool isAuthenticated)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            
            IEnumerable<CartItemDto> items;

            if (isAuthenticated)
            {
                parameters.Add("@EmployerId", int.Parse(cartIdentifier));
                
                items = await connection.QueryAsync<CartItemDto>(
                    "[dbo].[sp_GetCartItems]",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            else
            {
                parameters.Add("@SessionId", cartIdentifier);
                
                items = await connection.QueryAsync<CartItemDto>(
                    "[dbo].[sp_GetSessionCartItems]",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }

            var totalAmount = items.Sum(x => x.TotalAmount);
            var totalItems = items.Count();

            return new CartSummaryDto
            {
                Items = items.ToList(),
                TotalItems = totalItems,
                TotalAmount = totalAmount
            };
        }

        public async Task<bool> UpdateCartItemAsync(int cartId, int employerId,int laborId, int requiredHours, string? workDescription, DateTime? preferredDate)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@CartId", cartId);
            parameters.Add("@EmployerId", employerId);
            parameters.Add("@laborId", laborId);
            parameters.Add("@RequiredHours", requiredHours);
            parameters.Add("@WorkDescription", workDescription);
            parameters.Add("@PreferredDate", preferredDate);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_UpdateCartItem]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.RowsAffected > 0;
        }

        public async Task<bool> RemoveFromCartAsync(int cartId, int employerId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@CartId", cartId);
            parameters.Add("@EmployerId", employerId);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_RemoveFromCart]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.RowsAffected > 0;
        }

        public async Task<bool> ClearCartAsync(int employerId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@EmployerId", employerId);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_ClearCart]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.RowsAffected > 0;
        }

        public async Task<int> GetCartItemCountAsync(string cartIdentifier, bool isAuthenticated)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();

            if (isAuthenticated)
            {
                parameters.Add("@EmployerId", int.Parse(cartIdentifier));
                
                var result = await connection.QuerySingleAsync<dynamic>(
                    "[dbo].[sp_GetCartItemCount]",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                
                return result.ItemCount;
            }
            else
            {
                parameters.Add("@SessionId", cartIdentifier);
                
                var result = await connection.QuerySingleAsync<dynamic>(
                    "[dbo].[sp_GetSessionCartItemCount]",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                
                return result.ItemCount;
            }
        }

        public async Task<bool> MergeSessionCartAsync(string sessionId, string userId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@SessionId", sessionId);
            parameters.Add("@UserId", int.Parse(userId));

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_MergeSessionCart]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.Success == 1;
        }
    }
} 