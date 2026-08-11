using Dapper;
using Labor.DataAccess.Context;
using Labor.DataAccess.IRepositories;
using Labor.Models.DTOs.Admin;
using Labor.Models.Entities.User;
using System.Data;

namespace Labor.DataAccess.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly IDbContext _dbcontext;

        public AdminRepository(IDbContext dbcontext) {
            _dbcontext = dbcontext;
        }
        public async Task<int> CreateRoleAsync(string roleName, string? description, int? createdBy)
        {
            try
            {
                using var connection=_dbcontext.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@RoleName", roleName);
                parameters.Add("@Description", description);
                parameters.Add("@CreatedBy", createdBy);
                string procName = "[dbo].[sp_CreateRole]";

                var row = await connection.QuerySingleAsync<dynamic>(
                    procName,
                    parameters,
                    commandType: CommandType.StoredProcedure
                    );
                return Convert.ToInt32(row.NewRoleId);

            }
            catch (Exception ex)
            {

                throw new Exception("Exception on create role time");
            }
        }

        public async Task<bool> UpdateRoleAsync(int roleId, string roleName, string? description, bool isActive, int? updatedBy)
        {
            try
            {
                using var connections=_dbcontext.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@RoleId", roleId);
                parameters.Add("@RoleName", roleName);
                parameters.Add("@Description", description);
                parameters.Add("@IsActive", isActive);
                parameters.Add("@UpdatedBy", updatedBy);

                string procName = "[dbo].[sp_UpdateRole]";

                var result = await connections.QuerySingleAsync<dynamic>(
                    procName,
                    parameters,
                    commandType: CommandType.StoredProcedure
                    );

                return result.RowsAffected > 0;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> DeleteRoleAsync(int roleId, int? updatedBy)
        {
            try
            {
                using var connections=_dbcontext.CreateConnection(); 
                var parameters = new DynamicParameters();
                parameters.Add("@RoleId", roleId);
                parameters.Add("@UpdatedBy", updatedBy);
                string procName = "[dbo].[sp_DeleteRole]";

                var result = await connections.QuerySingleAsync(
                    procName,
                    parameters,
                    commandType: CommandType.StoredProcedure
                    );

                return result.RowsAffected > 0;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int> CreateRolePermissionAsync(int roleId, string featureName, bool canView, bool canCreate, bool canEdit, bool canDelete, int? createdBy)
        {
            using var connection = _dbcontext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@RoleID", roleId);
            parameters.Add("@FeatureName", featureName);
            parameters.Add("@CanView", canView);
            parameters.Add("@CanCreate", canCreate);
            parameters.Add("@CanEdit", canEdit);
            parameters.Add("@CanDelete", canDelete);
            parameters.Add("@CreatedBy", createdBy);

            string procName = "[dbo].[sp_CreateRolePermission]";
            var row = await connection.QuerySingleAsync<dynamic>(
                procName,
                parameters,
                commandType: CommandType.StoredProcedure);
            return Convert.ToInt32(row.NewPermissionId);
        }

        public async Task<List<RolePermissionResponseDto>> GetRolePermissionsByRoleIdAsync(int roleId)
        {
            using var connection = _dbcontext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@RoleID", roleId);
            const string procName = "[dbo].[sp_GetRolePermissions]";

            var rows = await connection.QueryAsync<dynamic>(
                procName,
                parameters,
                commandType: CommandType.StoredProcedure);

            var list = new List<RolePermissionResponseDto>();
            foreach (var row in rows)
            {
                list.Add(new RolePermissionResponseDto
                {
                    Id = (int)row.PermissionID,
                    RoleId = roleId,
                    RoleName = string.Empty,
                    FeatureName = (string)row.FeatureName,
                    CanView = (bool)row.CanView,
                    CanCreate = (bool)row.CanCreate,
                    CanEdit = (bool)row.CanEdit,
                    CanDelete = (bool)row.CanDelete,
                    IsActive = (bool)row.IsActive,
                });
            }

            return list;
        }

        public async Task<bool> UpdateRolePermissionByIdAsync(int permissionId, UpdateRolePermissionDto dto)
        {
            using var connection = _dbcontext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@PermissionId", permissionId);
            parameters.Add("@FeatureName", dto.FeatureName);
            parameters.Add("@CanView", dto.CanView);
            parameters.Add("@CanCreate", dto.CanCreate);
            parameters.Add("@CanEdit", dto.CanEdit);
            parameters.Add("@CanDelete", dto.CanDelete);
            parameters.Add("@IsActive", dto.IsActive);
            parameters.Add("@UpdatedBy", dto.UpdatedBy);

            string procName = "[dbo].[sp_UpdateRolePermissionById]";
            var result = await connection.QuerySingleAsync<dynamic>(
                procName,
                parameters,
                commandType: CommandType.StoredProcedure);
            return result.RowsAffected > 0;
        }

        public async Task<bool> DeleteRolePermissionAsync(int permissionId, int? updatedBy)
        {
            using var connection = _dbcontext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@PermissionId", permissionId);
            parameters.Add("@UpdatedBy", updatedBy);
            string procName = "[dbo].[sp_DeleteRolePermission]";
            var result = await connection.QuerySingleAsync<dynamic>(
                procName,
                parameters,
                commandType: CommandType.StoredProcedure);
            return result.RowsAffected > 0;
        }

        public async Task<List<RoleResponseDto>> GetRolesAsync()
        {
            using var connection= _dbcontext.CreateConnection();
            string procName = "[dbo].[sp_GetRoles]";
            var result = await connection.QueryAsync<RoleResponseDto>(
                procName,
                commandType: CommandType.StoredProcedure
                );
            return result.ToList();
        }

        public async Task<RoleResponseDto> GetRoleByIdAsync(int RoleId)
        {
            using var connection=_dbcontext.CreateConnection();
             string procName = "[dbo].[sp_GetRoles]";
            var parameters= new DynamicParameters();
            parameters.Add("@RoleId", RoleId);
            var result = await connection.QueryFirstOrDefaultAsync<RoleResponseDto>(
                procName,
                parameters,
                commandType: CommandType.StoredProcedure
                );
            return result;
        }

        public async Task<PermissionResponseDto?> GetPermissionByIdAsync(int permissionId)
        {
            using var connection = _dbcontext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@PermissionId", permissionId);
            string procName = "[dbo].[sp_GetPermissionById]";

            var row = await connection.QueryFirstOrDefaultAsync<dynamic>(
               procName,
                parameters,
                commandType: CommandType.StoredProcedure);
            if (row == null) return null;
            return new PermissionResponseDto
            {
                Id = (int)row.ID,
                RoleId = (int)row.RoleID,
                FeatureName = (string)row.FeatureName,
                CanView = (bool)row.CanView,
                CanCreate = (bool)row.CanCreate,
                CanEdit = (bool)row.CanEdit,
                CanDelete = (bool)row.CanDelete,
                IsActive = (bool)row.IsActive,
                CreatedAt = (DateTime)row.CreatedAt,
                CreatedBy=(int)row.createdBy,
                UpdatedAt=(DateTime)row.updatedBy,
                UpdatedBy=(int)row.updatedBy
            };
        }

        public async Task<bool> CreateUpdateRolePermissionByRoleIdAsync(int roleId, CreateUpdateRolePermissionsDto dto)
        {
            using var connection=_dbcontext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@RoleID", roleId);
            parameters.Add("@FeatureName", dto.FeatureName);
            parameters.Add("@CanView", dto.CanView);
            parameters.Add("@CanCreate", dto.CanCreate);
            parameters.Add("@CanEdit", dto.CanEdit);
            parameters.Add("@CanDelete", dto.CanDelete);
            parameters.Add("@CreatedBy", dto.CreatedBy);
            parameters.Add("@UpdatedBy", dto.UpdatedBy);
            string procName = "[dbo].[sp_CreateUpdateRolePermission]";

            var result=await connection.QuerySingleAsync<dynamic>(procName, parameters,commandType:CommandType.StoredProcedure);

            return result.PermissionId > 0;

        }
    }
}
