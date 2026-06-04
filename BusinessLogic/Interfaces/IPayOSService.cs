using PayOS.Models.Webhooks;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IPayOSService
    {
        Task<string> CreatePaymentLinkAsync(
            string orderCode,
            string planName,
            decimal amount);

        Task<WebhookData> VerifyWebhookDataAsync(Webhook webhookBody);
    }
}
