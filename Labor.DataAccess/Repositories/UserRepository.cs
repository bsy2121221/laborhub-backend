using Dapper;
using Labor.DataAccess.Context;
using Labor.DataAccess.IRepositories;
using Labor.Models.DTOs.Admin;
using Labor.Models.DTOs.Auth;
using Labor.Models.Entities.User;
using System.Data;

namespace Labor.DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbContext _context;

        public UserRepository(IDbContext context)
        {
            _context = context;
        }

        public async Task<bool> GetUserByUserName(string userName)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT COUNT(1) FROM [dbo].[Users] WHERE MobileNumber = @UserName AND IsActive = 1";
            var count = await connection.QuerySingleAsync<int>(sql, new { UserName = userName });
            return count > 0;
        }

        public async Task<User?> GetByMobileNumberAsync(string mobileNumber)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@MobileNumber", mobileNumber);

            var result = await connection.QueryAsync<User, Person, Role, User>(
                "[dbo].[sp_GetUserByMobileNumber]",
                (user, person, role) =>
                {
                    user.Person = person;
                    user.Role = role;
                    return user;
                },
                parameters,
                commandType: CommandType.StoredProcedure,
                splitOn: "ID,ID"
            );

            return result.FirstOrDefault();
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            var result = await connection.QueryAsync<User, Person, Role, User>(
                "[dbo].[sp_GetUserById]",
                (user, person, role) =>
                {
                    user.Person = person;
                    user.Role = role;
                    return user;
                },
                parameters,
                commandType: CommandType.StoredProcedure,
                splitOn: "ID,ID"
            );

            return result.FirstOrDefault();
        }

        public async Task<int> CreateAsync(User user)
        {
            using var connection = _context.CreateConnection();
            var sql = @"
                INSERT INTO [dbo].[Users] (
                    PersonID, MobileNumber, RoleID, PasswordHash, 
                    IsTemporaryPassword, IsActive, IsProfileComplete, CreatedAt
                )
                VALUES (
                    @PersonId, @MobileNumber, @RoleId, @PasswordHash, 
                    @IsTemporaryPassword, @IsActive, @IsProfileComplete, @CreatedAt
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var userId = await connection.QuerySingleAsync<int>(sql, user);
            return userId;
        }

        public async Task<int> CreateCompleteUserAsync(string mobileNumber, string passwordHash, int roleId, CompleteProfileDto profileData, bool isProfileComplete = true)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@MobileNumber", mobileNumber);
            parameters.Add("@PasswordHash", passwordHash);
            parameters.Add("@RoleId", roleId);
            parameters.Add("@FirstName", profileData.FirstName);
            parameters.Add("@LastName", profileData.LastName);
            parameters.Add("@Email", profileData.Email);
            parameters.Add("@Street", profileData.Street);
            parameters.Add("@City", profileData.City);
            parameters.Add("@State", profileData.State);
            parameters.Add("@Country", profileData.Country);
            parameters.Add("@ZipCode", profileData.ZipCode);
            parameters.Add("@Latitude", profileData.Latitude);
            parameters.Add("@Longitude", profileData.Longitude);
            parameters.Add("@IsProfileComplete", isProfileComplete);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_CreateCompleteUser]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.UserId;
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string passwordHash, bool isTemporaryPassword = false)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@PasswordHash", passwordHash);
            parameters.Add("@IsTemporaryPassword", isTemporaryPassword);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_UpdatePassword]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.RowsAffected > 0;
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_UpdateLastLogin]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.RowsAffected > 0;
        }

        public async Task<bool> UpdateAsync(User user)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", user.Id);
            parameters.Add("@MobileNumber", user.MobileNumber);
            parameters.Add("@RoleId", user.RoleId);
            parameters.Add("@PasswordHash", user.PasswordHash);
            parameters.Add("@IsTemporaryPassword", user.IsTemporaryPassword);
            parameters.Add("@IsActive", user.IsActive);
            parameters.Add("@IsProfileComplete", user.IsProfileComplete);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_UpdateUser]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.RowsAffected > 0;
        }

        public async Task<bool> IsProfileCompleteAsync(int userId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            var result = await connection.QuerySingleOrDefaultAsync<bool>(
                "[dbo].[sp_IsProfileComplete]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<bool> CompleteProfileAsync(int userId, CompleteProfileDto profileData)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@FirstName", profileData.FirstName);
            parameters.Add("@LastName", profileData.LastName);
            parameters.Add("@Email", profileData.Email);
            parameters.Add("@Street", profileData.Street);
            parameters.Add("@City", profileData.City);
            parameters.Add("@State", profileData.State);
            parameters.Add("@Country", profileData.Country);
            parameters.Add("@ZipCode", profileData.ZipCode);
            parameters.Add("@Latitude", profileData.Latitude);
            parameters.Add("@Longitude", profileData.Longitude);

            var result = await connection.QuerySingleAsync<dynamic>(
                "[dbo].[sp_CompleteProfile]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.Success == 1;
        }

        public async Task<IEnumerable<Role>> GetRolesAsync()
        {
            using var connection = _context.CreateConnection();
            
            return await connection.QueryAsync<Role>(
                "[dbo].[sp_GetRoles]",
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@RoleId", roleId);
            
            return await connection.QueryFirstOrDefaultAsync<Role>(
                "[dbo].[sp_GetRoleById]",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@RoleName", roleName);
            
            return await connection.QueryFirstOrDefaultAsync<Role>(
                "[dbo].[sp_GetRoleByName]",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<UserProfileDetailDto?> GetUserProfileDetailAsync(int loginUserId, int targetUserId)
        {
           var connection= _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@LoginUserId", loginUserId);
            parameters.Add("@TargetUserId", targetUserId);
            string procName = "[dbo].[sp_GetUserProfileDetails]";

            var result = await connection.QueryFirstOrDefaultAsync<UserProfileDetailDto>(
                procName,
                parameters,
                commandType: CommandType.StoredProcedure
                );
            return result;
        }

        public async Task<bool> UpdateUserProfileAsync(int loginUserId, int targetUserId, string? passwordHash, UpdateUserProfileDto request)
        {
            var connection= _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@LoginUserId", loginUserId);
            parameters.Add("@TargetUserId", targetUserId);
            parameters.Add("@UserName", request.UserName);
            parameters.Add("@MobileNumber", request.MobileNumber);
            parameters.Add("@FirstName", request.FirstName);
            parameters.Add("@LastName", request.LastName);
            parameters.Add("@Email", request.Email);
            parameters.Add("@ProfilePicture", request.ProfilePicture);
            parameters.Add("@PasswordHash", passwordHash);
            parameters.Add("@UpdatedBy", loginUserId);

            string procName = "[dbo].[sp_UpdateUserProfile]";

            var result = await connection.QuerySingleAsync<dynamic>(
                procName,
                parameters,
                commandType: CommandType.StoredProcedure);
            return result.RowsAffected>0;
        }
    }
}