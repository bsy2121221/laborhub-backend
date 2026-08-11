using Labor.Models.DTOs.Admin;
using Labor.Models.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labor.DataAccess.IRepositories
{
    public interface IAdminRepository
    {
        Task<int> CreateRoleAsync(string roleName,string? description,int? createdBy);
        Task<bool> UpdateRoleAsync(int roleId, string roleName, string? description, bool isActive, int? updatedBy);
        Task<bool> DeleteRoleAsync(int roleId,int? updatedBy);

        Task<int> CreateRolePermissionAsync(int roleId, string featureName, bool canView, bool canCreate, bool canEdit, bool canDelete, int? createdBy);
        Task<List<RolePermissionResponseDto>> GetRolePermissionsByRoleIdAsync(int roleId);
        Task<PermissionResponseDto?> GetPermissionByIdAsync(int roleId);
        Task<bool> UpdateRolePermissionByIdAsync(int permissionId, UpdateRolePermissionDto dto);

        Task<bool> CreateUpdateRolePermissionByRoleIdAsync(int roleId, CreateUpdateRolePermissionsDto dto);
        Task<bool> DeleteRolePermissionAsync(int permissionId, int? updatedBy);
        Task<List<RoleResponseDto>> GetRolesAsync();
        Task<RoleResponseDto> GetRoleByIdAsync(int RoleId);
    }
}
