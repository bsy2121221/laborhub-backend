using Labor.DataAccess.IServices;
using Labor.Models.DTOs.TelePhony;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;

namespace Labor.DataAccess.Services.Telephony;

public class ExotelIvrService : IIvrService
{
    private readonly HttpClient _http;
    private readonly ExotelOptions _options;
    private readonly ILogger<ExotelIvrService> _logger;

    public ExotelIvrService(HttpClient http, IOptions<ExotelOptions> options, ILogger<ExotelIvrService> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string?> PlaceLaborConfirmationCallAsync(
        int laborConfirmationId,
        string laborMobile,
        string employerName,
        string workArea,
        DateTime? scheduledDate,
        CancellationToken ct = default)
    {
        var url = $"https://{_options.Subdomain}/v1/Accounts/{_options.AccountSid}/Calls/connect.json";
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ApiKey}:{_options.ApiToken}"));

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);

        var callbackUrl = string.IsNullOrWhiteSpace(_options.StatusCallbackUrl)
            ? null
            : _options.StatusCallbackUrl;

        var form = new Dictionary<string, string>
        {
            ["From"] = laborMobile,
            ["CallerId"] = _options.CallerId,
            ["Url"] = $"http://my.exotel.com/{_options.AccountSid}/exoml/start_voice/{_options.IvrAppId}",
            ["CustomField"] = laborConfirmationId.ToString(),
        };

        if (!string.IsNullOrWhiteSpace(callbackUrl))
        {
            form["StatusCallback"] = callbackUrl;
        }

        request.Content = new FormUrlEncodedContent(form);

        var response = await _http.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Exotel call failed for confirmation {Id}: {Body}", laborConfirmationId, body);
            return null;
        }

        _logger.LogInformation("Exotel call placed for confirmation {Id}", laborConfirmationId);
        return body;
    }
}
