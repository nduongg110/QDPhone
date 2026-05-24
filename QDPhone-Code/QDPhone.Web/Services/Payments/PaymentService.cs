using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace QDPhone.Web.Services;

public interface IPaymentService
{
    Task<string> CreatePayOsCheckoutUrlAsync(int orderId, decimal amount, CancellationToken cancellationToken = default);
    bool VerifyPayOsSignature(int orderCode, string amount, string status, string signature);
}

public class PaymentService : IPaymentService
{
    private const string PayOsCreatePaymentEndpoint = "https://api-merchant.payos.vn/v2/payment-requests";
    private readonly PayOsOptions _options;
    private readonly HttpClient _httpClient;

    public PaymentService(IOptions<PayOsOptions> options, IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _httpClient = httpClientFactory.CreateClient(nameof(PaymentService));
    }

    public async Task<string> CreatePayOsCheckoutUrlAsync(int orderId, decimal amount, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId) ||
            string.IsNullOrWhiteSpace(_options.ApiKey) ||
            string.IsNullOrWhiteSpace(_options.ChecksumKey))
        {
            return BuildMockCallbackUrl(orderId, amount);
        }

        var roundedAmount = decimal.ToInt32(decimal.Round(amount, 0, MidpointRounding.AwayFromZero));
        var returnUrl = string.IsNullOrWhiteSpace(_options.ReturnUrl) ? "https://localhost:7010/Checkout/PayOsCallback" : _options.ReturnUrl;
        var cancelUrl = string.IsNullOrWhiteSpace(_options.CancelUrl) ? "https://localhost:7010/checkout/cancel" : _options.CancelUrl;
        var description = $"Thanh toan don {orderId}";
        if (description.Length > 25) description = description[..25];

        var request = new PayOsCreatePaymentRequest
        {
            OrderCode = orderId,
            Amount = roundedAmount,
            Description = description,
            ReturnUrl = returnUrl,
            CancelUrl = cancelUrl,
            Signature = SignCreateRequest(orderId, roundedAmount, description, returnUrl, cancelUrl)
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, PayOsCreatePaymentEndpoint)
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("x-client-id", _options.ClientId);
        httpRequest.Headers.Add("x-api-key", _options.ApiKey);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<PayOsCreatePaymentResponse>(cancellationToken: cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var message = payload?.Desc ?? $"PayOS HTTP {(int)response.StatusCode}";
            throw new InvalidOperationException($"Không tạo được link PayOS: {message}");
        }

        if (payload is null || payload.Code != "00" || string.IsNullOrWhiteSpace(payload.Data?.CheckoutUrl))
        {
            var message = payload?.Desc ?? "Dữ liệu trả về không hợp lệ";
            throw new InvalidOperationException($"Không tạo được link PayOS: {message}");
        }

        return payload.Data.CheckoutUrl;
    }

    private string BuildMockCallbackUrl(int orderId, decimal amount)
    {
        var status = "success";
        var amountText = amount.ToString("0.00", CultureInfo.InvariantCulture);
        var signature = Sign(orderId, amountText, status);
        var returnUrl = string.IsNullOrWhiteSpace(_options.ReturnUrl) ? "https://localhost:7010/Checkout/PayOsCallback" : _options.ReturnUrl;
        return $"{returnUrl}?orderCode={orderId}&amount={Uri.EscapeDataString(amountText)}&status={Uri.EscapeDataString(status)}&signature={signature}";
    }

    public bool VerifyPayOsSignature(int orderCode, string amount, string status, string signature)
    {
        var expected = Sign(orderCode, amount, status);
        return string.Equals(expected, signature, StringComparison.OrdinalIgnoreCase);
    }

    private string Sign(int orderCode, string amount, string status)
    {
        if (string.IsNullOrWhiteSpace(_options.ChecksumKey)) return string.Empty;
        var payload = $"{orderCode}|{amount}|{status}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.ChecksumKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private string SignCreateRequest(int orderCode, int amount, string description, string returnUrl, string cancelUrl)
    {
        if (string.IsNullOrWhiteSpace(_options.ChecksumKey)) return string.Empty;
        var payload = $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.ChecksumKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

