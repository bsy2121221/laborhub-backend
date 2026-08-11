using Labor.DataAccess.IRepositories;
using Labor.DataAccess.IServices;
using Labor.Models.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labor.DataAccess.Services
{
    public class AdminManagementService : IAdminManagementService
    {
        private readonly IAdminManagementRepository _adminManagementRepository;

        public AdminManagementService(IAdminManagementRepository adminManagementRepository)
        {
            this._adminManagementRepository = adminManagementRepository;
          
        }

        public async Task<bool> AdminUpdateLaborFullAsync(int laborId, AdminUpdateLaborRequestDto request, string? passwordHash, int? updatedBy)
        {
            return await _adminManagementRepository.AdminUpdateLaborFullAsync(laborId, request, passwordHash, updatedBy);
        }

        public async Task<List<AdminLaborListItemDto>> GetAllLaborsAsync(bool? verifiedOnly, int pageNumber, int pageSize)
        {
           return await _adminManagementRepository.GetAllLaborsAsync(verifiedOnly, pageNumber, pageSize);
        }

        public async Task<List<AdminOrderListItemDto>> GetAllOrdersAsync(string? orderStatus, int? pageNumber, int pageSize)
        {
            return await _adminManagementRepository.GetAllOrdersAsync(orderStatus, pageNumber, pageSize);
        }

        public async Task<AdminLaborDetailDto?> GetLaborForEditAsync(int laborId)
        {
            return await _adminManagementRepository.GetLaborForEditAsync(laborId);
        }

        public async Task<List<AdminUserListItemDto>> GetUsersAsync(string? role, bool inactiveUsers, int pageNumber, int pageSize)
        {
            return await _adminManagementRepository.GetUsersAsync(role, inactiveUsers, pageNumber, pageSize);
        }

        public async Task<OnboardLaborResponseDto> OnboardLaborAsync(OnboardLaborRequestDto request, string passwordHash, int? createdBy)
        {
            return await _adminManagementRepository.OnboardLaborAsync(request, passwordHash, createdBy);
        }

        public async Task<bool> SetLaborActiveAsync(int laborId, int? UpdatedBy, bool isActive)
        {
            return await _adminManagementRepository.SetLaborActiveAsync(laborId, UpdatedBy, isActive);
        }

        public async Task<bool> SetUserActiveAsync(int userId, int? UpdatedBy, bool active)
        {
            return await _adminManagementRepository.SetUserActiveAsync(userId, UpdatedBy, active);
        }

        public async Task<bool> VerifyLaborAsync(int laborId)
        {
            return await _adminManagementRepository.VerifyLabourAsync(laborId);
        }

        

        
    }
}
