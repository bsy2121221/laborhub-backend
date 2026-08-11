using Dapper;
using Labor.DataAccess.Context;
using Labor.DataAccess.IRepositories;
using Labor.Models.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labor.DataAccess.Repositories
{
    public class AdminManagementRepository : IAdminManagementRepository
    {
        private readonly IDbContext _dbContext;

        public AdminManagementRepository(IDbContext dbContext)
        {
            this._dbContext = dbContext;
        }
        public async Task<List<AdminUserListItemDto>> GetUsersAsync(string? role, bool inactiveUsers, int pageNumber, int pageSize)
        {
            using var connection = _dbContext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Role", role);
            parameters.Add("@InactiveUsers", inactiveUsers ? 1 : 0);
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            string procName = "[dbo].[sp_AdminGetUsers]";
            var result = await connection.QueryAsync<AdminUserListItemDto>(
                procName,
                parameters,
                commandType: CommandType.StoredProcedure
                );

            return result.ToList();
        }
        public async Task<List<AdminLaborListItemDto>> GetAllLaborsAsync(bool? verifiedOnly, int pageNumber, int pageSize)
        {
            using var connection = _dbContext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@VerifiedOnly", verifiedOnly);
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            var rows = await connection.QueryAsync<AdminLaborListItemDto>(
                "[dbo].[sp_AdminGetAllLabors]",
                parameters,
                commandType: CommandType.StoredProcedure);
            return rows.AsList();

        }

        public async Task<List<AdminOrderListItemDto>> GetAllOrdersAsync(string? orderStatus, int? pageNumber, int pageSize)
        {
            using var connection = _dbContext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@OrderStatus", orderStatus);
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            var rows = await connection.QueryAsync<AdminOrderListItemDto>(
                "[dbo].[sp_AdminGetAllOrders]",
                parameters,
                commandType: CommandType.StoredProcedure);
            return rows.AsList();
        }

       

        public async Task<OnboardLaborResponseDto> OnboardLaborAsync(OnboardLaborRequestDto request, string passwordHash, int? createdBy)
        {
            using var connection = _dbContext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@MobileNumber", request.MobileNumber.Trim());
            parameters.Add("@UserName", request.UserName.Trim());
            parameters.Add("@PasswordHash", passwordHash);
            parameters.Add("@FirstName", request.FirstName.Trim());
            parameters.Add("@LastName", request.LastName.Trim());
            parameters.Add("@Email", string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim());
            parameters.Add("@LaborTypeId", request.LaborTypeId);
            parameters.Add("@DailyRate", request.DailyRate);
            parameters.Add("@Specialization", string.IsNullOrWhiteSpace(request.Specialization) ? null : request.Specialization.Trim());
            parameters.Add("@ExperienceYears", request.ExperienceYears);
            parameters.Add("@MaximumHourAvilablePerDay", request.MaximumHourAvilablePerDay);
            parameters.Add("@CreatedBy", createdBy);
            parameters.Add("@Street", request.Street.Trim());
            parameters.Add("@City", request.City.Trim());
            parameters.Add("@State", request.State.Trim());
            parameters.Add("@Country", request.Country.Trim());
            parameters.Add("@ZipCode", request.ZipCode.Trim());
            parameters.Add("@Latitude", request.Latitude);
            parameters.Add("@Longitude", request.Longitude);
            parameters.Add("@ProfilePicture", string.IsNullOrWhiteSpace(request.ProfilePicture)? null : request.ProfilePicture.Trim()
        );
            var result = await connection.QuerySingleAsync<OnboardLaborResponseDto>(
                "[dbo].[sp_AdminOnboardLabor]",
                parameters,
                commandType: CommandType.StoredProcedure);
            return result;
        }

        public async Task<bool> SetUserActiveAsync(int userId,int? UpdatedBy, bool active)
        {
            using var connection = _dbContext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@IsActive", active);
            parameters.Add("@UpdatedBy", UpdatedBy);
            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_AdminSetUserActive]",
                parameters,
                commandType: CommandType.StoredProcedure);
            return result.RowsAffected > 0;
        }

        public async Task<bool> VerifyLabourAsync(int laborId)
        {
            using var connection = _dbContext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@LaborId", laborId);
            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_AdminVerifyLabor]",
                parameters,
                commandType: CommandType.StoredProcedure);
            return result.RowsAffected > 0;
        }

        public async Task<bool> SetLaborActiveAsync(int laborId, int? UpdatedBy, bool isActive)
        {
            using var connection= _dbContext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@LaborId", laborId);
            parameters.Add("@IsActive", isActive);
            parameters.Add("@UpdatedBy", UpdatedBy);
            var procName = "[dbo].[sp_AdminSetLaborActive]";


            var result= await connection.QuerySingleAsync<dynamic>( 
                procName,
                parameters,
                commandType: CommandType.StoredProcedure);
            return result.RowsAffected > 0;
        }

        public async Task<AdminLaborDetailDto?> GetLaborForEditAsync(int laborId)
        {
            using var connection = _dbContext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@LaborId", laborId);
            string procName = "[dbo].[sp_AdminGetLaborForEdit]";
            return await connection.QueryFirstOrDefaultAsync<AdminLaborDetailDto>(
               procName,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> AdminUpdateLaborFullAsync(int laborId, AdminUpdateLaborRequestDto request, string? passwordHash, int? updatedBy)
        {
            using var connection = _dbContext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@LaborId", laborId);
            parameters.Add("@MobileNumber", request.MobileNumber.Trim());
            parameters.Add("@UserName", string.IsNullOrWhiteSpace(request.UserName) ? null : request.UserName.Trim());
            parameters.Add("@FirstName", request.FirstName.Trim());
            parameters.Add("@LastName", request.LastName.Trim());
            parameters.Add("@Email", string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim());
            parameters.Add("@ProfilePicture", string.IsNullOrWhiteSpace(request.ProfilePicture) ? null : request.ProfilePicture.Trim());
            parameters.Add("@PasswordHash", passwordHash);
            parameters.Add("@Street", request.Street.Trim());
            parameters.Add("@City", request.City.Trim());
            parameters.Add("@State", request.State.Trim());
            parameters.Add("@Country", request.Country.Trim());
            parameters.Add("@ZipCode", request.ZipCode.Trim());
            parameters.Add("@Latitude", request.Latitude);
            parameters.Add("@Longitude", request.Longitude);
            parameters.Add("@LaborTypeId", request.LaborTypeId);
            parameters.Add("@Specialization", string.IsNullOrWhiteSpace(request.Specialization) ? null : request.Specialization.Trim());
            parameters.Add("@ExperienceYears", request.ExperienceYears);
            parameters.Add("@DailyRate", request.DailyRate);
            parameters.Add("@MinimumHours", request.MinimumHours);
            parameters.Add("@MaximumHours", request.MaximumHours);
            parameters.Add("@AvailabilityStatus", string.IsNullOrWhiteSpace(request.AvailabilityStatus) ? "Available" : request.AvailabilityStatus.Trim());
            parameters.Add("@UpdatedBy", updatedBy);

            await connection.ExecuteAsync(
                "[dbo].[sp_AdminUpdateLaborFull]",
                parameters,
                commandType: CommandType.StoredProcedure);
            return true;
        }
    }
}
