using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Labor.Models.DTOs.Auth;
using Labor.Models.DTOs.Common;
using Labor.Models.Entities.User;
using System.Security.Claims;
using Labor.DataAccess.IRepositories;
using Labor.Auth.IAuthservice;

namespace Labor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserRepository userRepository,
            ITokenService tokenService,
            IPasswordService passwordService,
            ILogger<AuthController> logger)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordService = passwordService;
            _logger = logger;
        }


        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("Validation failed", errors));
                }

                // Check if user already exists
                var existingUser = await _userRepository.GetByMobileNumberAsync(request.MobileNumber);
                if (existingUser != null)
                {
                    return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("User with this mobile number already exists. Please login."));
                }

                // Self-service registration is employers only; labor onboarding is admin-managed.
                var role = await _userRepository.GetRoleByNameAsync("Employer");
                if (role == null)
                {
                    return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("Employer role not found"));
                }

                // Hash the password
                var hashedPassword = _passwordService.HashPassword(request.Password);

                // Placeholder profile until name/address are collected at checkout or complete-profile
                const string pending = "Pending";
                var completeProfile = new CompleteProfileDto
                {
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Email = null,
                    Street = pending,
                    City = pending,
                    State = pending,
                    Country = "India",
                    ZipCode = pending,
                    Latitude = null,
                    Longitude = null
                };

                var userId = await _userRepository.CreateCompleteUserAsync(
                    request.MobileNumber,
                    hashedPassword,
                    role.Id,
                    completeProfile,
                    isProfileComplete: false);

                // Get the created user for token generation
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("Failed to create user"));
                }

                // Generate JWT token
                var token = _tokenService.GenerateToken(user, role.RoleName);

                var response = new AuthResponseDto
                {
                    UserId = user.Id,
                    UserName = $"+91 {request.MobileNumber}",
                    Role = role.RoleName,
                    Token = token,
                    TokenExpiry = _tokenService.GetTokenExpiry(token),
                    IsTemporaryPassword = false,
                    RequirePasswordChange = false,
                    IsProfileComplete = false
                };

                return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(response, "Registration successful. Welcome to Labor Management!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {MobileNumber}", request.MobileNumber);
                return StatusCode(500, ApiResponse<AuthResponseDto>.ErrorResponse("Internal server error"));
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("Validation failed", errors));
                }

                // Get user by mobile number
                var user = await _userRepository.GetByMobileNumberAsync(request.MobileNumber);
                if (user == null)
                {
                    return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("Account not found. Please register first."));
                }

                // Verify password
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("Account setup is incomplete. Please contact support."));
                }

                var passwordValid = _passwordService.VerifyPassword(request.Password, user.PasswordHash);
                if (!passwordValid)
                {
                    return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("Invalid mobile number or password"));
                }

                // Update last login
                await _userRepository.UpdateLastLoginAsync(user.Id);

                // Generate JWT token
                var token = _tokenService.GenerateToken(user, user.Role.RoleName);

                var response = new AuthResponseDto
                {
                    UserId = user.Id,
                    UserName = user.Person?.FirstName + " " + user.Person?.LastName ?? user.MobileNumber,
                    Role = user.Role.RoleName,
                    Token = token,
                    TokenExpiry = _tokenService.GetTokenExpiry(token),
                    IsTemporaryPassword = false,
                    RequirePasswordChange = false,
                    IsProfileComplete = user.IsProfileComplete
                };

                return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(response, "Login successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in user {MobileNumber}", request.MobileNumber);
                return StatusCode(500, ApiResponse<AuthResponseDto>.ErrorResponse("Internal server error"));
            }
        }

        [HttpPost("complete-profile")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> CompleteProfile([FromBody] CompleteProfileDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse.ErrorResponse("Validation failed", errors));
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                // Check if profile is already complete
                var isComplete = await _userRepository.IsProfileCompleteAsync(userId);
                if (isComplete)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Profile is already complete"));
                }

                // Complete the profile
                var success = await _userRepository.CompleteProfileAsync(userId, request);
                if (!success)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Failed to complete profile"));
                }

                return Ok(ApiResponse.SuccessResponse("Profile completed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing profile for user");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse.ErrorResponse("Validation failed", errors));
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(ApiResponse.ErrorResponse("User not found"));
                }

                // Verify current password if it exists
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    var currentPasswordValid = _passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash);
                    if (!currentPasswordValid)
                    {
                        return BadRequest(ApiResponse.ErrorResponse("Current password is incorrect"));
                    }
                }

                // Hash new password
                var hashedNewPassword = _passwordService.HashPassword(request.NewPassword);

                // Update password
                await _userRepository.UpdatePasswordAsync(userId, hashedNewPassword, false);

                return Ok(ApiResponse.SuccessResponse("Password changed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }
    }
} 