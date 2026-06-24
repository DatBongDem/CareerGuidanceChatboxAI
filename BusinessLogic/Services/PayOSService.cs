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
    }
}
