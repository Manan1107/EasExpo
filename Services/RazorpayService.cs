using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EasExpo.Models.Options;
using Microsoft.Extensions.Options;

namespace EasExpo.Services
{
    public class RazorpayService : IRazorpayService
    {
        private readonly RazorpayOptions _options;
        private readonly HttpClient _httpClient;

        public RazorpayService(HttpClient httpClient, IOptions<RazorpayOptions> optionsAccessor)
        {
            _options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
            if (string.IsNullOrWhiteSpace(_options.KeyId) || string.IsNullOrWhiteSpace(_options.KeySecret))
            {
                throw new InvalidOperationException("Razorpay keys are not configured. Please set Razorpay:KeyId and Razorpay:KeySecret in appsettings.");
            }

            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri("https://api.razorpay.com/v1/");
            }

            var credentials = Encoding.ASCII.GetBytes($"{_options.KeyId}:{_options.KeySecret}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));
        }

        public async Task<RazorpayOrderResult> CreateOrderAsync(decimal amount, string currency, string receiptId)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
            }

            var payload = new
            {
                amount = (int)Math.Round(amount * 100m, MidpointRounding.AwayFromZero),
                currency,
                receipt = receiptId,
                payment_capture = 1
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("orders", content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            return new RazorpayOrderResult
            {
                OrderId = root.GetProperty("id").GetString(),
                Currency = root.GetProperty("currency").GetString(),
                Amount = root.GetProperty("amount").GetInt32() / 100m,
                Receipt = root.GetProperty("receipt").GetString()
            };
        }

        public Task<bool> VerifyPaymentAsync(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature)
        {
            if (string.IsNullOrWhiteSpace(razorpayOrderId) || string.IsNullOrWhiteSpace(razorpayPaymentId) || string.IsNullOrWhiteSpace(razorpaySignature))
            {
                return Task.FromResult(false);
            }

            var payload = $"{razorpayOrderId}|{razorpayPaymentId}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.KeySecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var generatedSignature = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            return Task.FromResult(generatedSignature == razorpaySignature);
        }
    }
}
