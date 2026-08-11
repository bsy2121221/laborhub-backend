using Labor.Models.DTOs.Admin;
using Labor.Models.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labor.DataAccess.IServices
{
    public interface IAdminRoleService
    {
        Task<int> CreateRoleAsync(string roleName, string? description, int? createdBy);
        Task<bool> UpdateRoleAsync(int roleId, string roleName, string? description, bool isActive, int? updatedBy);
        Task<bool> DeleteRoleAsync(int roleId, int? updatedBy);
        Task<int> CreateRolePermissionAsync(CreateRolePermissionDto dto);
        Task<List<RolePermissionResponseDto>> GetRolePermissionsByRoleIdAsync(int roleId);
        Task<PermissionResponseDto?> GetPermissionByIdAsync(int roleId);
        Task<bool> UpdateRolePermissionByIdAsync(int Permissionid,UpdateRolePermissionDto dto);
        Task<bool> CreateUpdateRolePermissionByRoleIdAsync(int roleId, CreateUpdateRolePermissionsDto dto);
        Task<bool> DeleteRolePermissionAsync(int permissionId, int? updatedBy);
        Task<List<RoleResponseDto>> GetRolesAsync();
        Task<RoleResponseDto> GetRoleByIdAsync(int RoleId);
    }
}
