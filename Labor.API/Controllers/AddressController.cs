using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Labor.Models.DTOs.Order;
using Labor.Models.DTOs.Common;
using Labor.Models.Entities.User;
using System.Security.Claims;
using Labor.DataAccess.IRepositories;

namespace Labor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ILogger<AddressController> _logger;

        public AddressController(IAddressRepository addressRepository, ILogger<AddressController> logger)
        {
            _addressRepository = addressRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<Address>>>> GetMyAddresses()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var addresses = await _addressRepository.GetUserAddressesAsync(userId);
                
                return Ok(ApiResponse<IEnumerable<Address>>.SuccessResponse(addresses, "Addresses retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user addresses");
                return StatusCode(500, ApiResponse<IEnumerable<Address>>.ErrorResponse("Internal server error"));
            }
        }

        [HttpGet("{addressId}")]
        public async Task<ActionResult<ApiResponse<Address>>> GetAddress(int addressId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var address = await _addressRepository.GetAddressByIdAsync(addressId, userId);
                
                if (address == null)
                {
                    return NotFound(ApiResponse<Address>.ErrorResponse("Address not found"));
                }

                return Ok(ApiResponse<Address>.SuccessResponse(address, "Address retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address {AddressId}", addressId);
                return StatusCode(500, ApiResponse<Address>.ErrorResponse("Internal server error"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<int>>> CreateAddress([FromBody] AddressDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<int>.ErrorResponse("Validation failed", errors));
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                var address = new Address
                {
                    UserId = userId,
                    AddressType = request.AddressType,
                    Street = request.Street,
                    City = request.City,
                    State = request.State,
                    Country = request.Country,
                    ZipCode = request.ZipCode,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    IsDefault = request.IsDefault,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var addressId = await _addressRepository.CreateAddressAsync(address);
                
                return Ok(ApiResponse<int>.SuccessResponse(addressId, "Address created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address");
                return StatusCode(500, ApiResponse<int>.ErrorResponse("Internal server error"));
            }
        }

        [HttpPut("{addressId}")]
        public async Task<ActionResult<ApiResponse>> UpdateAddress(int addressId, [FromBody] AddressDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse.ErrorResponse("Validation failed", errors));
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                var address = new Address
                {
                    Id = addressId,
                    UserId = userId,
                    AddressType = request.AddressType,
                    Street = request.Street,
                    City = request.City,
                    State = request.State,
                    Country = request.Country,
                    ZipCode = request.ZipCode,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude
                };

                var updated = await _addressRepository.UpdateAddressAsync(address);
                
                if (!updated)
                {
                    return NotFound(ApiResponse.ErrorResponse("Address not found"));
                }

                return Ok(ApiResponse.SuccessResponse("Address updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }

        [HttpDelete("{addressId}")]
        public async Task<ActionResult<ApiResponse>> DeleteAddress(int addressId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var deleted = await _addressRepository.DeleteAddressAsync(addressId, userId);
                
                if (!deleted)
                {
                    return NotFound(ApiResponse.ErrorResponse("Address not found"));
                }

                return Ok(ApiResponse.SuccessResponse("Address deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }

        [HttpPut("{addressId}/set-default")]
        public async Task<ActionResult<ApiResponse>> SetDefaultAddress(int addressId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var updated = await _addressRepository.SetDefaultAddressAsync(addressId, userId);
                
                if (!updated)
                {
                    return NotFound(ApiResponse.ErrorResponse("Address not found"));
                }

                return Ok(ApiResponse.SuccessResponse("Default address set successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default address");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }

        [HttpGet("default")]
        public async Task<ActionResult<ApiResponse<Address>>> GetDefaultAddress()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var address = await _addressRepository.GetDefaultAddressAsync(userId);
                
                if (address == null)
                {
                    return NotFound(ApiResponse<Address>.ErrorResponse("No default address found"));
                }

                return Ok(ApiResponse<Address>.SuccessResponse(address, "Default address retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default address");
                return StatusCode(500, ApiResponse<Address>.ErrorResponse("Internal server error"));
            }
        }
    }
} 