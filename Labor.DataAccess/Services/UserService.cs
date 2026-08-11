using Labor.DataAccess.IRepositories;
using Labor.DataAccess.IServices;
using Labor.Models.DTOs.Admin;

namespace Labor.DataAccess.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            this._userRepository = userRepository;
        }
        public Task<UserProfileDetailDto?> GetUserProfileDetailAsync(int loginUserId, int targetUserId)
        {
            return _userRepository.GetUserProfileDetailAsync(loginUserId, targetUserId);
        }

        public Task<bool> UpdateUserProfileAsync(int loginUserId, int targetUserId, string? passwordHash, UpdateUserProfileDto request)
        {
            return _userRepository.UpdateUserProfileAsync(loginUserId, targetUserId, passwordHash, request);
        }
    }
}
