using Labor.DataAccess.IServices;
using Labor.Models.DTOs.Common;
using Labor.Models.DTOs.TelePhony;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Labor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IvrController : ControllerBase
{
    private readonly ILaborConfirmationService _laborConfirmationService;
    private readonly ExotelOptions _exotelOptions;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<IvrController> _logger;

    public IvrController(
        ILaborConfirmationService laborConfirmationService,
        IOptions<ExotelOptions> exotelOptions,
        IWebHostEnvironment environment,
        ILogger<IvrController> logger)
    {
        _laborConfirmationService = laborConfirmationService;
        _exotelOptions = exotelOptions.Value;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Exotel Passthru / digit callback. CustomField must contain LaborConfirmationID.
    /// </summary>
    [HttpPost("exotel/digit")]
    [AllowAnonymous]
    public async Task<IActionResult> ExotelDigit(
        [FromForm] string? Digits,
        [FromForm] string? CustomField,
        [FromForm] string? CallSid)
    {
        if (!ValidateWebhookSecret())
        {
            return Unauthorized();
        }

        if (!int.TryParse(CustomField, out var confirmationId))
        {
            _logger.LogWarning("Exotel digit callback missing CustomField. CallSid={CallSid}", CallSid);
            return BadRequest();
        }

        if (Digits == "9")
        {
            return Content("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Response><Redirect>replay</Redirect></Response>", "application/xml");
        }

        if (Digits is not ("1" or "2"))
        {
            return Ok();
        }

        var result = await _laborConfirmationService.ProcessIvrDigitAsync(confirmationId, Digits);
        if (!result.Success)
        {
            return BadRequest(ApiResponse.ErrorResponse("Could not process IVR response."));
        }

        return Ok();
    }

    /// <summary>
    /// Local / dev testing without a phone call.
    /// </summary>
    [HttpPost("mock-response")]
    [AllowAnonymous]
    public async Task<IActionResult> MockResponse([FromBody] MockIvrRequestDto request)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var result = await _laborConfirmationService.ProcessIvrDigitAsync(request.LaborConfirmationId, request.Digit);
        if (!result.Success)
        {
            return BadRequest(ApiResponse.ErrorResponse("Invalid or already processed confirmation."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            result.OrderId,
            result.LaborId,
            request.Digit
        }, "IVR response processed."));
    }

    private bool ValidateWebhookSecret()
    {
        if (string.IsNullOrWhiteSpace(_exotelOptions.WebhookSecret))
        {
            return true;
        }

        if (Request.Headers.TryGetValue("X-Webhook-Secret", out var headerValue))
        {
            return headerValue == _exotelOptions.WebhookSecret;
        }

        if (Request.Query.TryGetValue("secret", out var queryValue))
        {
            return queryValue == _exotelOptions.WebhookSecret;
        }

        return false;
    }
}
