using Labor.Models.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labor.DataAccess.IRepositories
{
    public interface IAdminManagementRepository
    {
        Task<List<AdminUserListItemDto>> GetUsersAsync(string? role, bool inactiveUsers, int pageNumber, int pageSize);
        Task<bool> SetUserActiveAsync(int  userId,int? UpdatedBy, bool active);
        Task<bool> VerifyLabourAsync(int laborId);
        Task<bool> SetLaborActiveAsync(int laborId, int? UpdatedBy, bool isActive);
        Task<OnboardLaborResponseDto> OnboardLaborAsync(OnboardLaborRequestDto request, string passwordHash, int? createdBy);
        Task<List<AdminOrderListItemDto>> GetAllOrdersAsync(string? orderStatus,int ? pageNumber,int pageSize);
        Task<List<AdminLaborListItemDto>> GetAllLaborsAsync(bool? verifiedOnly, int pageNumber, int pageSize);
        Task<AdminLaborDetailDto?> GetLaborForEditAsync(int laborId);
        Task<bool> AdminUpdateLaborFullAsync(int laborId, AdminUpdateLaborRequestDto request, string? passwordHash, int? updatedBy);
        Task<bool?> AdminCreateUpdateLaborTypesAsync(int? createdBy, int? updatedBy, AdminCreateUpdateLaborTypes request);
    }
}
