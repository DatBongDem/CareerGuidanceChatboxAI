using PayOS.Models.Webhooks;
using PayOS.Models.V2.PaymentRequests;
using System.Threading.Tasks;
using BusinessLogic.DTOs.Payment;

namespace BusinessLogic.Interfaces
{
    public interface IPayOSService
    {
        Task<CreatePaymentLinkResponse> CreatePaymentLinkAsync(
            string orderCode,
            string planName,
            decimal amount);

        Task<WebhookData> VerifyWebhookDataAsync(Webhook webhookBody);
        Task<PaymentLinkInformation> GetPaymentLinkInformationAsync(string orderCode);
    }
}
