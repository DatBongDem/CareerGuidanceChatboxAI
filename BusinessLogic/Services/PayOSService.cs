using BusinessLogic.Configurations;
using BusinessLogic.Interfaces;
using BusinessLogic.DTOs.Payment;
using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Models.Webhooks;
using PayOS.Models.V2.PaymentRequests;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class PayOSService : IPayOSService
    {
        private readonly PayOSClient _payOS;
        private readonly PayOSSettings _settings;

        public PayOSService(IOptions<PayOSSettings> options)
        {
            _settings = options.Value;

            _payOS = new PayOSClient(
                _settings.ClientId,
                _settings.ApiKey,
                _settings.ChecksumKey
            );
        }

        public async Task<CreatePaymentLinkResponse> CreatePaymentLinkAsync(
            string orderCode,
            string planName,
            decimal amount)
        {
            long payOSOrderCode = long.Parse(orderCode);

            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = payOSOrderCode,
                Amount = (long)amount,
                Description = orderCode,
                Items = new List<PaymentLinkItem>
                {
                    new PaymentLinkItem
                    {
                        Name = planName,
                        Quantity = 1,
                        Price = (long)amount
                    }
                },
                CancelUrl = _settings.CancelUrl,
                ReturnUrl = _settings.ReturnUrl,
                ExpiredAt = DateTimeOffset.UtcNow.AddDays(14).ToUnixTimeSeconds()
            };

            var result = await _payOS.PaymentRequests.CreateAsync(paymentRequest);
            return result;
        }

        public async Task<WebhookData> VerifyWebhookDataAsync(Webhook webhookBody)
        {
            return await _payOS.Webhooks.VerifyAsync(webhookBody);
        }

        public async Task<PaymentLinkInformation> GetPaymentLinkInformationAsync(string orderCode)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-client-id", _settings.ClientId);
            client.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);

            string url = $"https://api-merchant.payos.vn/v2/payment-requests/{orderCode}";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;
            
            if (root.TryGetProperty("code", out var codeProp) && codeProp.GetString() == "00")
            {
                var data = root.GetProperty("data");
                return new PaymentLinkInformation
                {
                    Bin = data.TryGetProperty("bin", out var bin) ? bin.GetString() ?? "" : "",
                    AccountNumber = data.TryGetProperty("accountNumber", out var accNum) ? accNum.GetString() ?? "" : "",
                    AccountName = data.TryGetProperty("accountName", out var accName) ? accName.GetString() ?? "" : "",
                    Amount = data.TryGetProperty("amount", out var amt) ? amt.GetInt64() : 0,
                    Description = data.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                    OrderCode = data.TryGetProperty("orderCode", out var oc) ? oc.GetInt64() : 0,
                    Currency = data.TryGetProperty("currency", out var curr) ? curr.GetString() ?? "" : "",
                    PaymentLinkId = data.TryGetProperty("paymentLinkId", out var plId) ? plId.GetString() ?? "" : "",
                    Status = data.TryGetProperty("status", out var status) ? status.GetString() ?? "" : "",
                    CheckoutUrl = data.TryGetProperty("checkoutUrl", out var urlProp) ? urlProp.GetString() ?? "" : "",
                    QrCode = data.TryGetProperty("qrCode", out var qr) ? qr.GetString() ?? "" : ""
                };
            }
            
            throw new ApplicationException($"Lỗi khi lấy thông tin thanh toán từ PayOS: {root.GetProperty("desc").GetString()}");
        }
    }
}
