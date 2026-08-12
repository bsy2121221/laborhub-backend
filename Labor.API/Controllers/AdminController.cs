using Labor.DataAccess.IServices;
using Labor.Models.DTOs.Admin;
using Labor.Models.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Labor.API.Utilities;
using Microsoft.Data.SqlClient;
using Labor.Models.Entities.User;
using Labor.Auth.IAuthservice;

namespace Labor.API.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminRoleService _adminRoleService;
        private readonly IAdminManagementService _adminManagementService;
        private readonly IPasswordService _passwordService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAdminRoleService adminRoleService,
            IAdminManagementService adminManagementService,
            IPasswordService passwordService,
            IWebHostEnvironment webHostEnvironment,
            ILogger<AdminController> logger
            )
        {
            _adminRoleService = adminRoleService;
            _adminManagementService = adminManagementService;
            _passwordService = passwordService;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        [HttpGet("roles")]
        public async Task<ActionResult<ApiResponse<List<RoleResponseDto>>>> GetRoles()
        {
            try
            {
                var result = await _adminRoleService.GetRolesAsync();
                if (result == null)
                    return NotFound(ApiResponse<List<RoleResponseDto>>.ErrorResponse("roles not found"));
                return Ok(ApiResponse<List<RoleResponseDto>>.SuccessResponse(result, "OK"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles");
                return StatusCode(500, ApiResponse<List<RoleResponseDto>>.ErrorResponse("Internal server error"));
            }
        }
        [HttpGet("role/{RoleId:int}")]
        public async Task<ActionResult<RoleResponseDto>> GetRole(int RoleId)
        {
            try
            {
                var result = await _adminRoleService.GetRoleByIdAsync(RoleId);
                if (result == null)
                    return NotFound(ApiResponse<RoleResponseDto>.ErrorResponse("Role not found"));
                return Ok(ApiResponse<RoleResponseDto>.SuccessResponse(result, "OK"));
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error getting role");
                return StatusCode(500, ApiResponse<RoleResponseDto>.ErrorResponse("Internal server error"));
            }
        }

        [HttpPost("roles")]
        public async Task<ActionResult<ApiResponse<int>>> CreateRole([FromBody] CreateRolesDto createRolesDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<int>.ErrorResponse("Validation failed", errors));
                }

                if(createRolesDto.CreatedBy==0 || createRolesDto.CreatedBy==null)
                    createRolesDto.CreatedBy = User.GetCurrentUserId();

                var newRoleId = await _adminRoleService.CreateRoleAsync(createRolesDto.RoleName, createRolesDto.Description, createRolesDto.CreatedBy);
                return Ok(ApiResponse<int>.SuccessResponse(newRoleId, "Role Created Successfully"));
            }
            catch (SqlException ex)
            {
                _logger.LogWarning(ex, "Creation Failed");
                return Conflict(ApiResponse<int>.ErrorResponse(ex.Message));
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error creating role");
                return StatusCode(500,ApiResponse<int>.ErrorResponse("Internal server error"));
            }
        }
        [HttpPut("roles/{roleId:int}")]
        public async Task<ActionResult<ApiResponse>> UpdateRole(int roleId, [FromBody] UpdateRoleDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse.ErrorResponse("Validation Faield", errors));
                }
                if (request.UpdatedBy == 0 ||request.UpdatedBy==null)
                    request.UpdatedBy=User.GetCurrentUserId();

                var result=await _adminRoleService.UpdateRoleAsync(roleId, request.RoleName,request.Description,request.IsActive, request.UpdatedBy);
                if (!result)
                {
                    return NotFound(ApiResponse.ErrorResponse("Role not Found"));
                }
                return Ok(ApiResponse.SuccessResponse("Role updated successfully"));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role");
                return StatusCode(500,ApiResponse.ErrorResponse("Internal server error"));
                
            }
        }
        [HttpDelete("roles/{roleId:int}")]
        public async Task<ActionResult<ApiResponse>> DeleteRole(int roleId)
        {
            try
            {

                var updatedBy = User.GetCurrentUserId();
                var result = await _adminRoleService.DeleteRoleAsync(roleId, updatedBy);

                if (!result)
                {
                    return NotFound(ApiResponse.ErrorResponse("Role not FOund"));
                }
                return Ok(ApiResponse.SuccessResponse("Role Deleted Successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));

            }
        }


        [HttpPost("permissions")]
        public async Task<ActionResult<ApiResponse<int>>> CreateRolePermission([FromBody] CreateRolePermissionDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<int>.ErrorResponse("Validation failed", errors));
                }
                if (request.CreatedBy == null || request.CreatedBy == 0)
                    request.CreatedBy = User.GetCurrentUserId();
                var id = await _adminRoleService.CreateRolePermissionAsync(request);
                return Ok(ApiResponse<int>.SuccessResponse(id, "Role permission created successfully"));
            }
            catch (SqlException ex)
            {
                _logger.LogWarning(ex, "CreateRolePermission failed");
                return Conflict(ApiResponse<int>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role permission");
                return StatusCode(500, ApiResponse<int>.ErrorResponse("Internal server error"));
            }
        }

        [HttpPost("role-permissions/{roleId:int}")]
        public async Task<ActionResult<ApiResponse<int>>> CreateUpdateRolePermissions(int roleId,[FromBody] CreateUpdateRolePermissionsDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<int>.ErrorResponse("Validation failed", errors));
                }
                if (request.CreatedBy == null || request.CreatedBy == 0)
                    request.CreatedBy = User.GetCurrentUserId();
                var id = await _adminRoleService.CreateUpdateRolePermissionByRoleIdAsync(roleId,request);
                return Ok(ApiResponse<bool>.SuccessResponse(id, "Role permissions created successfully"));
            }
            catch (SqlException ex)
            {
                _logger.LogWarning(ex, "CreateRolePermission failed");
                return Conflict(ApiResponse<int>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role permission");
                return StatusCode(500, ApiResponse<int>.ErrorResponse("Internal server error"));
            }
        }

        [HttpGet("permissions/{permissionId:int}")]
        public async Task<ActionResult<ApiResponse<PermissionResponseDto>>> GetPermission(int permissionId)
        {
            try
            {
                var row = await _adminRoleService.GetPermissionByIdAsync(permissionId);
                if (row == null)
                    return NotFound(ApiResponse<PermissionResponseDto>.ErrorResponse("Permission not found"));
                return Ok(ApiResponse<PermissionResponseDto>.SuccessResponse(row, "OK"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading role permission");
                return StatusCode(500, ApiResponse<PermissionResponseDto>.ErrorResponse("Internal server error"));
            }
        }


        [HttpGet("role-permissions/{roleId:int}")]
        public async Task<ActionResult<ApiResponse<List<RolePermissionResponseDto>>>> GetRolePermissions(int roleId)
        {
            try
            {
                var rows = await _adminRoleService.GetRolePermissionsByRoleIdAsync(roleId);
                return Ok(ApiResponse<List<RolePermissionResponseDto>>.SuccessResponse(rows, "OK"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading role permissions");
                return StatusCode(500, ApiResponse<List<RolePermissionResponseDto>>.ErrorResponse("Internal server error"));
            }
        }

        [HttpPut("role-permissions/{permissionId:int}")]
        public async Task<ActionResult<ApiResponse>> UpdateRolePermission(int permissionId, [FromBody] UpdateRolePermissionDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse.ErrorResponse("Validation failed", errors));
                }
                if (request.UpdatedBy == null || request.UpdatedBy == 0)
                    request.UpdatedBy = User.GetCurrentUserId();
                var ok = await _adminRoleService.UpdateRolePermissionByIdAsync(permissionId, request);
                if (!ok)
                    return NotFound(ApiResponse.ErrorResponse("Role permission not found"));
                return Ok(ApiResponse.SuccessResponse("Role permission updated successfully"));
            }
            catch (SqlException ex)
            {
                _logger.LogWarning(ex, "UpdateRolePermission failed");
                return Conflict(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role permission");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }


        [HttpDelete("role-permissions/{permissionId:int}")]
        public async Task<ActionResult<ApiResponse>> DeleteRolePermission(int permissionId)
        {
            try
            {
                var updatedBy = User.GetCurrentUserId();
                var ok = await _adminRoleService.DeleteRolePermissionAsync(permissionId, updatedBy);
                if (!ok)
                    return NotFound(ApiResponse.ErrorResponse("Role permission not found"));
                return Ok(ApiResponse.SuccessResponse("Role permission deleted successfully"));
            }
            catch (SqlException ex)
            {
                _logger.LogWarning(ex, "DeleteRolePermission failed");
                return Conflict(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role permission");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }

        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminUserListItemDto>>>> GetUsers(
            [FromQuery] string? role,
            [FromQuery] bool inactiveUsers = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var list = await _adminManagementService.GetUsersAsync(role, inactiveUsers, pageNumber, pageSize);
                return Ok(ApiResponse<IReadOnlyList<AdminUserListItemDto>>.SuccessResponse(list, "OK"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin GetUsers failed");
                return StatusCode(500, ApiResponse<IReadOnlyList<AdminUserListItemDto>>.ErrorResponse("Internal server error"));
            }
        }

        [HttpPost("onboard-labor")]
        public async Task<ActionResult<ApiResponse<OnboardLaborResponseDto>>> OnboardLabor([FromBody] OnboardLaborRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<OnboardLaborResponseDto>.ErrorResponse("Validation failed", errors));
                }
                var hash = _passwordService.HashPassword(request.Password);
                var createdBy = User.GetCurrentUserId();
                var result = await _adminManagementService.OnboardLaborAsync(request, hash, createdBy);
               // var dto = new OnboardLaborResponseDto { UserId = result.UserId, LaborId = result.LaborId };
                return Ok(ApiResponse<OnboardLaborResponseDto>.SuccessResponse(result, "Labor onboarded successfully"));
            }
            catch (SqlException ex)
            {
                _logger.LogWarning(ex, "OnboardLabor failed");
                return Conflict(ApiResponse<OnboardLaborResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OnboardLabor error");
                return StatusCode(500, ApiResponse<OnboardLaborResponseDto>.ErrorResponse("Internal server error"));
            }
        }

        [HttpPost("upload-labor-photo")]
        [RequestFormLimits(MultipartBodyLengthLimit = 5 * 1024 * 1024)]
        public async Task<ActionResult<ApiResponse<UploadLaborPhotoResponseDto>>> UploadLaborPhoto(IFormFile? file,CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<UploadLaborPhotoResponseDto>.ErrorResponse("No file uploaded."));

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            string[] allowed = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
            if (!allowed.Contains(ext))
                return BadRequest(ApiResponse<UploadLaborPhotoResponseDto>.ErrorResponse("Invalid file type."));

            var webroot = _webHostEnvironment.WebRootPath ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
            var uploadDir = Path.Combine(webroot, "uploads", "labor-photos");
            Directory.CreateDirectory(uploadDir);

            var safeName = $"{Guid.NewGuid():N}{ext}";
            var physicalPath = Path.Combine(uploadDir, safeName);

            await using (var stream = System.IO.File.Create(physicalPath))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var relative = $"/uploads/labor-photos/{safeName}".Replace("\\", "/");
            var absolute = $"{Request.Scheme}://{Request.Host}{relative}";

            var dto = new UploadLaborPhotoResponseDto
            {
                RelativeUrl = relative,
                AbsoluteUrl = absolute
            };

            return Ok(ApiResponse<UploadLaborPhotoResponseDto>.SuccessResponse(dto, "Photo uploaded."));
        }

        [HttpPut("user/{userId:int}/activate")]
        public async Task<ActionResult<ApiResponse>> ActivateUser(int userId)
        {
            try
            {
                int? updatedBy=User.GetCurrentUserId();
                var ok = await _adminManagementService.SetUserActiveAsync(userId,updatedBy,true);
                if (!ok) return NotFound(ApiResponse.ErrorResponse("User not found"));
                return Ok(ApiResponse.SuccessResponse("User activated"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ActivateUser failed");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }

        [HttpPut("user/{userId:int}/deactivate")]
        public async Task<ActionResult<ApiResponse>> DeactivateUser(int userId)
        {
            try
            {
                int? updatedBy=User.GetCurrentUserId();
                var result = await _adminManagementService.SetUserActiveAsync(userId, updatedBy,false);
                if (!result) return NotFound(ApiResponse.ErrorResponse("User not found"));
                return Ok(ApiResponse.SuccessResponse("User deactivated"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeactivateUser failed");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }

        [HttpPut("labor/{laborId:int}/verify")]
        public async Task<ActionResult<ApiResponse>> VerifyLabor(int laborId)
        {
            try
            {
                var ok = await _adminManagementService.VerifyLaborAsync(laborId);
                if (!ok) return NotFound(ApiResponse.ErrorResponse("Labor not found"));
                return Ok(ApiResponse.SuccessResponse("Labor verified"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VerifyLabor failed");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }

        [HttpPut("labor/{laborId:int}/activate")]
        public async Task<ActionResult<ApiResponse>> ActivateLabor(int laborId)
        {
            try
            {
                int? updatedBy=User.GetCurrentUserId() ;
                var result = await _adminManagementService.SetLaborActiveAsync(laborId,updatedBy,true);
                if (!result) return NotFound(ApiResponse.ErrorResponse("Labor not found"));
                return Ok(ApiResponse.SuccessResponse("Labor Activated"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Labor Activation failed");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }

        [HttpPut("labor/{laborId:int}/deactivate")]
        public async Task<ActionResult<ApiResponse>> DeactivateLabor(int laborId)
        {
            try
            {
                int? updatedBy = User.GetCurrentUserId();
                var result = await _adminManagementService.SetLaborActiveAsync(laborId,updatedBy,false);
                if (!result) return NotFound(ApiResponse.ErrorResponse("Labor not found"));
                return Ok(ApiResponse.SuccessResponse("Labor Deactivated"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Labor Decativation failed");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }

        [HttpGet("orders")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminOrderListItemDto>>>> GetAllOrders([FromQuery] string? orderStatus,[FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 50)
        {
            try
            {
                var list = await _adminManagementService.GetAllOrdersAsync(orderStatus, pageNumber, pageSize);
                return Ok(ApiResponse<IReadOnlyList<AdminOrderListItemDto>>.SuccessResponse(list, "OK"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin GetAllOrders failed");
                return StatusCode(500, ApiResponse<IReadOnlyList<AdminOrderListItemDto>>.ErrorResponse("Internal server error"));
            }
        }

        [HttpGet("labors")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminLaborListItemDto>>>> GetAllLabors([FromQuery] bool? verifiedOnly,[FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 50)
        {
            try
            {
                var list = await _adminManagementService.GetAllLaborsAsync(verifiedOnly, pageNumber, pageSize);
                return Ok(ApiResponse<IReadOnlyList<AdminLaborListItemDto>>.SuccessResponse(list, "OK"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin GetAllLabors failed");
                return StatusCode(500, ApiResponse<IReadOnlyList<AdminLaborListItemDto>>.ErrorResponse("Internal server error"));
            }
        }

        [HttpGet("labor/{laborId:int}/details")]
        public async Task<ActionResult<ApiResponse<AdminLaborDetailDto>>> GetLaborForEdit(int laborId)
        {
            try
            {
                var result = await _adminManagementService.GetLaborForEditAsync(laborId);
                if (result == null)
                    return NotFound(ApiResponse<AdminLaborDetailDto>.ErrorResponse("Labor not found"));
                return Ok(ApiResponse<AdminLaborDetailDto>.SuccessResponse(result, "OK"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetLaborForEdit failed");
                return StatusCode(500, ApiResponse<AdminLaborDetailDto>.ErrorResponse("Internal server error"));
            }
        }

        [HttpPut("labor/{laborId:int}")]
        public async Task<ActionResult<ApiResponse>> UpdateLaborFull(int laborId, [FromBody] AdminUpdateLaborRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse.ErrorResponse("Validation failed", errors));
                }

                string? passwordHash = null;
                if (!string.IsNullOrWhiteSpace(request.NewPassword))
                    passwordHash = _passwordService.HashPassword(request.NewPassword);

                var updatedBy = User.GetCurrentUserId();
                await _adminManagementService.AdminUpdateLaborFullAsync(laborId, request, passwordHash, updatedBy);
                return Ok(ApiResponse.SuccessResponse("Labor updated successfully"));
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                _logger.LogWarning(ex, "UpdateLaborFull failed");
                return Conflict(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateLaborFull failed");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }
        [HttpPost("createOrUpdate-laborTypes")]
        public async Task<ActionResult<ApiResponse>> CreateOrUpdateLaborTypes([FromBody] AdminCreateUpdateLaborTypes adminCreateUpdateLaborTypes)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse.ErrorResponse("Validation failed", errors));
                }
                var createdBy = User.GetCurrentUserId();
                await _adminManagementService.AdminCreateUpdateLaborTypesAsync(createdBy, createdBy, adminCreateUpdateLaborTypes);
                return Ok(ApiResponse.SuccessResponse("Labor Types created successfully"));

            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                _logger.LogWarning(ex, "UpdateLaborFull failed");
                return Conflict(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {

                _logger.LogWarning(ex, "Create update of labor Types failed");
                return StatusCode(500,ApiResponse.ErrorResponse("Internal server error"));
            }
        }

    }
}
