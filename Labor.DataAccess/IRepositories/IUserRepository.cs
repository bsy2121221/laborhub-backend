using Labor.Models.DTOs.Admin;
using Labor.Models.DTOs.Auth;
using Labor.Models.Entities.User;

namespace Labor.DataAccess.IRepositories
{
    public interface IUserRepository
    {
        Task<bool> GetUserByUserName(string userName);
        Task<User?> GetByMobileNumberAsync(string mobileNumber);
        Task<User?> GetByIdAsync(int userId);
        Task<int> CreateAsync(User user);
        Task<int> CreateCompleteUserAsync(string mobileNumber, string passwordHash, int roleId, CompleteProfileDto profileData, bool isProfileComplete = true);
        Task<bool> UpdatePasswordAsync(int userId, string passwordHash, bool isTemporaryPassword = false);
        Task<bool> UpdateLastLoginAsync(int userId);
        Task<bool> UpdateAsync(User user);
        Task<bool> IsProfileCompleteAsync(int userId);
        Task<bool> CompleteProfileAsync(int userId, CompleteProfileDto profileData);
        Task<IEnumerable<Role>> GetRolesAsync();
        Task<Role?> GetRoleByIdAsync(int roleId);
        Task<Role?> GetRoleByNameAsync(string roleName);
        Task<UserProfileDetailDto?> GetUserProfileDetailAsync(int loginUserId, int targetUserId);
        Task<bool> UpdateUserProfileAsync(int loginUserId, int targetUserId, string? passwordHash, UpdateUserProfileDto request);
    }
} 