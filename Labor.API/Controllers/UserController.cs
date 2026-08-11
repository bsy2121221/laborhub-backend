using Labor.API.Utilities;
using Labor.Auth.IAuthservice;
using Labor.DataAccess.IServices;
using Labor.Models.DTOs.Admin;
using Labor.Models.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Labor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService,IPasswordService passwordService,ILogger<UserController> logger)
        {
            _userService = userService;
            _passwordService = passwordService;
            _logger = logger;
        }

        [HttpGet("User/{userId:int}/profile")]
        public async Task<ActionResult<ApiResponse<UserProfileDetailDto>>> GetUserProfile(int userId)
        {
            try
            {
                var loginUserId = User.GetCurrentUserId();
                if (loginUserId is null)
                    return Unauthorized(ApiResponse<UserProfileDetailDto>.ErrorResponse("Unauthorized"));

                var result = await _userService.GetUserProfileDetailAsync(loginUserId.Value, userId);

                if (result == null)
                    return NotFound(ApiResponse<UserProfileDetailDto>.ErrorResponse("User not found"));

                return Ok(ApiResponse<UserProfileDetailDto>.SuccessResponse(result, "ok"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserProfile failed for {UserId}", userId);
                return StatusCode(500, ApiResponse<UserProfileDetailDto>.ErrorResponse("Internal server error"));
            }
        }

        [HttpPut("user/{userId:int}/profile")]
        public async Task<ActionResult<ApiResponse>> UpdateUserProfile(int userId, [FromBody] UpdateUserProfileDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse.ErrorResponse("Validation failed", errors));
                }
                var loginUserId = User.GetCurrentUserId();
                if (loginUserId is null)
                    return Unauthorized(ApiResponse.ErrorResponse("Unauthorized"));

                var passwordCandidate = string.IsNullOrWhiteSpace(request.Password)
                    ? request.NewPassword
                    : request.Password;
                var confirmPassword = string.IsNullOrWhiteSpace(request.Password)
                    ? request.ConfirmPassword
                    : request.Password;
                string? hashedNewPassword = null;

                if (!string.IsNullOrWhiteSpace(passwordCandidate))
                {
                    if (passwordCandidate.Length < 6)
                        return BadRequest(ApiResponse.ErrorResponse("Password must be at least 6 characters"));

                    if (!string.Equals(passwordCandidate, confirmPassword, StringComparison.Ordinal))
                        return BadRequest(ApiResponse.ErrorResponse("Password confirmation does not match."));

                    hashedNewPassword = _passwordService.HashPassword(passwordCandidate);
                }

                await _userService.UpdateUserProfileAsync(loginUserId.Value, userId, hashedNewPassword, request);
                return Ok(ApiResponse.SuccessResponse("User updated successfully"));
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                _logger.LogWarning(ex, "UpdateUserProfile failed");
                return Conflict(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateUserProfile error");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }
    }
}
