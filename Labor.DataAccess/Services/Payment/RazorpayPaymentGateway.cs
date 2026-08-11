using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Labor.DataAccess.IServices;
using Labor.Models.DTOs.Payment;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Labor.DataAccess.Services.Payment;

public class RazorpayPaymentGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly RazorpayOptions _options;
    private readonly ILogger<RazorpayPaymentGateway> _logger;

    public RazorpayPaymentGateway(
        HttpClient httpClient,
        IOptions<RazorpayOptions> options,
        ILogger<RazorpayPaymentGateway> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public string ProviderName => "Razorpay";

    public async Task<GatewayOrderResult> CreateOrderAsync(decimal totalAmountInr, string receipt, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(_options.KeyId) || string.IsNullOrWhiteSpace(_options.KeySecret))
        {
            throw new InvalidOperationException("Razorpay KeyId and KeySecret must be configured.");
        }

        var amountPaise = (int)Math.Round(totalAmountInr * 100m, MidpointRounding.AwayFromZero);
        var payload = new
        {
            amount = amountPaise,
            currency = "INR",
            receipt,
            notes = string.IsNullOrWhiteSpace(notes) ? null : new { detail = notes }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.razorpay.com/v1/orders");
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.KeyId}:{_options.KeySecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Razorpay order creation failed: {Status} {Body}", response.StatusCode, body);
            throw new InvalidOperationException("Unable to create Razorpay order. Check API credentials.");
        }

        using var doc = JsonDocument.Parse(body);
        var orderId = doc.RootElement.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("Razorpay response missing order id.");

        return new GatewayOrderResult
        {
            Provider = ProviderName,
            ProviderOrderId = orderId,
            AmountPaise = amountPaise
        };
    }

    public bool VerifyPaymentSignature(string gatewayOrderId, string gatewayPaymentId, string signature)
    {
        if (string.IsNullOrWhiteSpace(_options.KeySecret) || string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        var payload = $"{gatewayOrderId}|{gatewayPaymentId}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.KeySecret));
        var hash = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
        return hash.Equals(signature, StringComparison.OrdinalIgnoreCase);
    }
}
