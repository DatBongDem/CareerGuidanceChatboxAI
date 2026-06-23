using BusinessLogic.Configurations;
using BusinessLogic.Interfaces;
using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Models.Webhooks;
using PayOS.Models.V2.PaymentRequests;
using System;
using System.Collections.Generic;
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

            // Sanitize description: max 25 characters, no Vietnamese accents, only basic chars
            string rawDescription = $"Thanh toan {planName}";
            string sanitizedDescription = SanitizeDescription(rawDescription);

            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = payOSOrderCode,
                Amount = (long)amount,
                Description = sanitizedDescription,
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
                ExpiredAt = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds()
            };

            var result = await _payOS.PaymentRequests.CreateAsync(paymentRequest);
            return result;
        }

        private string SanitizeDescription(string text)
        {
            if (string.IsNullOrEmpty(text)) return "Thanh toan";

            // Remove Vietnamese accents
            string normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (char c in normalized)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            string noAccents = sb.ToString().Normalize(System.Text.NormalizationForm.FormC);

            // Replace special characters: keep only letters, numbers, spaces, and hyphens
            string clean = System.Text.RegularExpressions.Regex.Replace(noAccents, @"[^a-zA-Z0-9\s-]", "");

            // Replace multiple spaces with a single space
            clean = System.Text.RegularExpressions.Regex.Replace(clean, @"\s+", " ").Trim();

            // Truncate to maximum of 25 characters
            if (clean.Length > 25)
            {
                clean = clean.Substring(0, 25).Trim();
            }

            return clean;
        }

        public async Task<WebhookData> VerifyWebhookDataAsync(Webhook webhookBody)
        {
            return await _payOS.Webhooks.VerifyAsync(webhookBody);
        }
    }
}
