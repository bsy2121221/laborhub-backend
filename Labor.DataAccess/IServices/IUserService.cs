using Labor.Models.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labor.DataAccess.IServices
{
    public interface IUserService
    {
        Task<UserProfileDetailDto?> GetUserProfileDetailAsync(int loginUserId, int targetUserId);
        Task<bool> UpdateUserProfileAsync(int loginUserId, int targetUserId, string? passwordHash, UpdateUserProfileDto request);
    }
}
