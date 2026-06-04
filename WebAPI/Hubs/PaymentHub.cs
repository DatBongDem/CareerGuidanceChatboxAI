using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace WebAPI.Hubs
{
    public class PaymentHub : Hub
    {
        public async Task JoinPaymentGroup(string transactionCode)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, transactionCode);
        }
    }
}
