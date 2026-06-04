using BusinessLogic.DTOs.Payment;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PayOS.Models.Webhooks;
using System.Security.Claims;
using WebAPI.Hubs;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IPayOSService _payOSService;
        private readonly IHubContext<PaymentHub> _hubContext;

        public PaymentController(
            IPaymentService paymentService,
            IPayOSService payOSService,
            IHubContext<PaymentHub> hubContext)
        {
            _paymentService = paymentService;
            _payOSService = payOSService;
            _hubContext = hubContext;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment(CreatePaymentRequestDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim);

            var result = await _paymentService.CreatePaymentAsync(userId, request.PlanId);

            return Ok(result);
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> ReceiveWebhook([FromBody] Webhook body)
        {
            try
            {
                var verifiedData = await _payOSService.VerifyWebhookDataAsync(body);
                string transactionCode = verifiedData.OrderCode.ToString();

                // Handle PayOS test webhook (orderCode is usually 123)
                if (transactionCode == "123")
                {
                    return Ok(new { message = "Test webhook processed successfully" });
                }

                if (verifiedData.Code == "00")
                {
                    try
                    {
                        await _paymentService.ConfirmPaymentAsync(transactionCode);
                    }
                    catch (ApplicationException ex) when (ex.Message == "Payment already confirmed")
                    {
                        // Already confirmed, proceed to notify client
                    }
                    catch (ApplicationException ex) when (ex.Message == "Transaction not found")
                    {
                        return Ok(new { message = $"Transaction {transactionCode} not found, acknowledged." });
                    }

                    // Notify realtime clients
                    await _hubContext.Clients.Group(transactionCode)
                        .SendAsync("PaymentConfirmed", new { status = "Success" });

                    return Ok(new { message = "Webhook processed successfully" });
                }
                else
                {
                    try
                    {
                        await _paymentService.CancelPaymentAsync(transactionCode);
                    }
                    catch (ApplicationException ex) when (ex.Message == "Payment has already been cancelled." || ex.Message == "Cannot cancel a successful payment.")
                    {
                        // Already cancelled or successful, proceed to notify client
                    }
                    catch (ApplicationException ex) when (ex.Message == "Transaction not found")
                    {
                        return Ok(new { message = $"Transaction {transactionCode} not found, acknowledged." });
                    }

                    // Notify realtime clients
                    await _hubContext.Clients.Group(transactionCode)
                        .SendAsync("PaymentFailed", new { status = "Failed", message = verifiedData.Description });

                    return Ok(new { message = $"Webhook processed (Payment failed/cancelled: {verifiedData.Description})" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
