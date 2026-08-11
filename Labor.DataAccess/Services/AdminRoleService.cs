using Labor.DataAccess.IRepositories;
using Labor.DataAccess.IServices;
using Labor.Models.DTOs.Admin;
using Labor.Models.Entities.User;

namespace Labor.DataAccess.Services
{
    public class AdminRoleService : IAdminRoleService
    {
        public readonly IAdminRepository _AdminRepository;
        public AdminRoleService(IAdminRepository adminRepository)
        {
            _AdminRepository = adminRepository;
        }

        public async Task<int> CreateRoleAsync(string roleName, string? description, int? createdBy)
        {
            return await _AdminRepository.CreateRoleAsync(roleName, description, createdBy);
        }
        public async Task<bool> UpdateRoleAsync(int roleId, string roleName, string? description, bool isActive, int? updatedBy)
        {
            return await _AdminRepository.UpdateRoleAsync(roleId, roleName, description, isActive, updatedBy);
        }
        public async Task<bool> DeleteRoleAsync(int roleId, int? updatedBy)
        {
           return await _AdminRepository.DeleteRoleAsync(roleId, updatedBy);
        }
        public async Task<int> CreateRolePermissionAsync(CreateRolePermissionDto dto)
        {
            return await _AdminRepository.CreateRolePermissionAsync(dto.RoleId, dto.FeatureName, dto.CanView, dto.CanCreate, dto.CanEdit, dto.CanDelete, dto.CreatedBy);
        }

        public async Task<bool> DeleteRolePermissionAsync(int permissionId, int? updatedBy)
        {
            return await _AdminRepository.DeleteRolePermissionAsync(permissionId, updatedBy);
        }

        public async Task<List<RolePermissionResponseDto>> GetRolePermissionsByRoleIdAsync(int roleId)
        {
            return await _AdminRepository.GetRolePermissionsByRoleIdAsync(roleId);
        }
        public async Task<bool> UpdateRolePermissionByIdAsync(int Permissionid,UpdateRolePermissionDto dto)
        {
            return await _AdminRepository.UpdateRolePermissionByIdAsync(Permissionid, dto);
        }

        public async Task<List<RoleResponseDto>> GetRolesAsync()
        {
           return await _AdminRepository.GetRolesAsync();
        }

        public async Task<RoleResponseDto> GetRoleByIdAsync(int RoleId)
        {
            return await _AdminRepository.GetRoleByIdAsync(RoleId);
        }

        public async Task<PermissionResponseDto?> GetPermissionByIdAsync(int roleId)
        {
           return await _AdminRepository.GetPermissionByIdAsync(roleId);
        }

        public async Task<bool> CreateUpdateRolePermissionByRoleIdAsync(int roleId, CreateUpdateRolePermissionsDto dto)
        {
            return await _AdminRepository.CreateUpdateRolePermissionByRoleIdAsync(roleId, dto);
        }
    }
}
