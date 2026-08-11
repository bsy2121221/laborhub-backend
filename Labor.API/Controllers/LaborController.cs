using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Labor.Models.DTOs.Labor;
using Labor.Models.DTOs.Common;
using Labor.Models.Entities.Labor;
using System.Security.Claims;
using Labor.DataAccess.IRepositories;
using Labor.DataAccess.IServices;

namespace Labor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LaborController : ControllerBase
    {
      
        private readonly ILaborService _laborService;
        private readonly ILogger<LaborController> _logger;

        public LaborController(ILaborService laborService, ILogger<LaborController> logger)
        {
           
            _laborService = laborService;
            _logger = logger;
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LaborResponseDto>>>> SearchLabors([FromQuery] LaborSearchRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<IEnumerable<LaborResponseDto>>.ErrorResponse("Validation failed", errors));
                }

                var labors = await _laborService.SearchLaborsAsync(request);
                return Ok(ApiResponse<IEnumerable<LaborResponseDto>>.SuccessResponse(labors, "Labors retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching labors");
                return StatusCode(500, ApiResponse<IEnumerable<LaborResponseDto>>.ErrorResponse("Internal server error"));
            }
        }

        [HttpGet("nearby")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LaborResponseDto>>>> GetNearbyLabors(
            [FromQuery] decimal latitude, 
            [FromQuery] decimal longitude, 
            [FromQuery] int radiusKm = 10,
            [FromQuery] int? laborTypeId = null)
        {
            try
            {
                var request = new LaborSearchRequestDto
                {
                    Latitude = latitude,
                    Longitude = longitude,
                    RadiusKm = radiusKm,
                    LaborTypeId = laborTypeId,
                    AvailabilityStatus = "Available",
                    PageSize = 50 // Show more results for nearby search
                };

                var labors = await _laborService.SearchLaborsAsync(request);
                return Ok(ApiResponse<IEnumerable<LaborResponseDto>>.SuccessResponse(labors, $"Found {labors.Count()} available labors within {radiusKm}km"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting nearby labors for location {Latitude}, {Longitude}", latitude, longitude);
                return StatusCode(500, ApiResponse<IEnumerable<LaborResponseDto>>.ErrorResponse("Internal server error"));
            }
        }

        [HttpGet("nearby-tomorrow")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LaborResponseDto>>>> GetNearbyLaborsAvailableTomorrow(
            [FromQuery] decimal latitude,
            [FromQuery] decimal longitude,
            [FromQuery] int radiusKm = 10,
            [FromQuery] string availabilityStatus = "Available",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var labors = await _laborService.GetAvailableLaborNearByTomorrowAsync(
                    latitude,
                    longitude,
                    radiusKm,
                    availabilityStatus,
                    pageNumber,
                    pageSize);

                return Ok(ApiResponse<IEnumerable<LaborResponseDto>>.SuccessResponse(
                    labors,
                    $"Found {labors.Count()} labor professionals available tomorrow within {radiusKm}km"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tomorrow nearby labors for location {Latitude}, {Longitude}", latitude, longitude);
                return StatusCode(500, ApiResponse<IEnumerable<LaborResponseDto>>.ErrorResponse("Internal server error"));
            }
        }

        [HttpGet("{laborId}")]
        public async Task<ActionResult<ApiResponse<LaborResponseDto>>> GetLaborDetails(int laborId)
        {
            try
            {
                var labor = await _laborService.GetLaborDetailsAsync(laborId);
                if (labor == null)
                {
                    return NotFound(ApiResponse<LaborResponseDto>.ErrorResponse("Labor not found"));
                }

                return Ok(ApiResponse<LaborResponseDto>.SuccessResponse(labor, "Labor details retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting labor details for {LaborId}", laborId);
                return StatusCode(500, ApiResponse<LaborResponseDto>.ErrorResponse("Internal server error"));
            }
        }

        [HttpGet("types")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LaborType>>>> GetLaborTypes()
        {
            try
            {
                var laborTypes = await _laborService.GetLaborTypesAsync();
                return Ok(ApiResponse<IEnumerable<LaborType>>.SuccessResponse(laborTypes, "Labor types retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting labor types");
                return StatusCode(500, ApiResponse<IEnumerable<LaborType>>.ErrorResponse("Internal server error"));
            }
        }

        [HttpPut("{laborId}/availability-status")]
        [Authorize(Roles = "Labor,Admin")]
        public async Task<ActionResult<ApiResponse>> UpdateAvailabilityStatus(int laborId, [FromBody] UpdateAvailabilityDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse.ErrorResponse("Validation failed", errors));
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                // Check if user owns this labor profile or is admin
                if (userRole != "Admin")
                {
                    var labor = await _laborService.GetLaborDetailsAsync(laborId);
                    if (labor?.UserId != userId)
                    {
                        return Forbid("You can only update your own availability");
                    }
                }

                var updated = await _laborService.UpdateLaborAvailabilityStatusAsync(laborId, request.AvailabilityStatus);
                if (!updated)
                {
                    return NotFound(ApiResponse.ErrorResponse("Labor not found"));
                }

                return Ok(ApiResponse.SuccessResponse("Availability updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating labor availability");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }

        [HttpPost("{laborId}/skills")]
        [Authorize(Roles = "Labor,Admin")]
        public async Task<ActionResult<ApiResponse>> AddSkill(int laborId, [FromBody] LaborSkillDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse.ErrorResponse("Validation failed", errors));
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                // Check if user owns this labor profile or is admin
                if (userRole != "Admin")
                {
                    var labor = await _laborService.GetLaborDetailsAsync(laborId);
                    if (labor?.UserId != userId)
                    {
                        return Forbid("You can only manage your own skills");
                    }
                }

                var skill = new LaborSkill
                {
                    LaborId = laborId,
                    SkillName = request.SkillName,
                    ProficiencyLevel = request.ProficiencyLevel
                };

                var added = await _laborService.AddLaborSkillAsync(skill);
                if (!added)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Failed to add skill"));
                }

                return Ok(ApiResponse.SuccessResponse("Skill added successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding labor skill");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }

        [HttpDelete("skills/{skillId}")]
        [Authorize(Roles = "Labor,Admin")]
        public async Task<ActionResult<ApiResponse>> RemoveSkill(int skillId)
        {
            try
            {
                var removed = await _laborService.RemoveLaborSkillAsync(skillId);
                if (!removed)
                {
                    return NotFound(ApiResponse.ErrorResponse("Skill not found"));
                }

                return Ok(ApiResponse.SuccessResponse("Skill removed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing labor skill");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));
            }
        }

        [HttpPut("{laborId}/availabilities")]
        [Authorize(Roles ="Labor,Admin")]
        public async Task<ActionResult<ApiResponse>> UpsertAvailabilities(int laborId, [FromBody] UpsertLaborAvailabilityRequestDto requestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors=ModelState.Values.SelectMany(v=>v.Errors).Select(v=>v.ErrorMessage).ToList();
                    return BadRequest(ApiResponse.ErrorResponse("Validation Failed", errors));
                }
                if(requestDto.Items==null || requestDto.Items.Count<=0)
                    return BadRequest(ApiResponse.ErrorResponse("At least one availablity item is required"));

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (userRole != "Admin")
                {
                    var labor = await _laborService.GetLaborDetailsAsync(laborId);
                    if (labor?.UserId != userId)
                        return Forbid("You can only update your own availability");
                }

                var result=await _laborService.UpsertLaborAvailabilitiesAsync(laborId,requestDto.Items);
                if(!result)
                    return NotFound(ApiResponse.ErrorResponse("Labor not found"));

                return Ok(ApiResponse.SuccessResponse("Availability updated successfully"));


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating labor availabilities");
                return StatusCode(500, ApiResponse.ErrorResponse("Internal server error"));

            }
        }
        [HttpGet("{laborId}/availabilities")]
        [Authorize(Roles = "Labor,Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LaborAvailabilityItemDto>>>> GetAvailabilitiesByMonth(
    int laborId,
    [FromQuery] int year,
    [FromQuery] int month)
        {
            try
            {
                if (year < 2000 || year > 2100 || month < 1 || month > 12)
                    return BadRequest(ApiResponse<IEnumerable<LaborAvailabilityItemDto>>.ErrorResponse("Invalid year/month"));

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (userRole != "Admin")
                {
                    var labor = await _laborService.GetLaborDetailsAsync(laborId);
                    if (labor?.UserId != userId)
                        return Forbid("You can only view your own availability");
                }

                var data = await _laborService.GetLaborAvailabilitiesByMonthAsync(laborId, year, month);
                return Ok(ApiResponse<IEnumerable<LaborAvailabilityItemDto>>.SuccessResponse(data, "OK"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading labor availabilities");
                return StatusCode(500, ApiResponse<IEnumerable<LaborAvailabilityItemDto>>.ErrorResponse("Internal server error"));
            }
        }

    }

    public class UpdateAvailabilityDto
    {
        public string AvailabilityStatus { get; set; } = string.Empty;
    }
} 