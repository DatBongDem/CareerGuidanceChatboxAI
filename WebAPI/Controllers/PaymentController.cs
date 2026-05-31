using BusinessLogic.DTOs.Payment;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    
        [ApiController]
        [Route("api/payment")]
        public class PaymentController : ControllerBase
        {
            private readonly IPaymentService
                _paymentService;

            public PaymentController(
                IPaymentService paymentService)
            {
                _paymentService = paymentService;
            }

            [HttpPost("create")]
            public async Task<IActionResult>
                CreatePayment(
                    CreatePaymentRequestDto request)
            {
            var userIdClaim = User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim);

            var result =
                    await _paymentService
                        .CreatePaymentAsync(
                            userId,
                            request.PlanId);

                return Ok(result);
            }

            [HttpPost("confirm")]
            public async Task<IActionResult>
                ConfirmPayment(string code)
            {
                await _paymentService
                    .ConfirmPaymentAsync(code);

                return Ok(new
                {
                    message = "Payment success"
                });
            }

            [HttpPost("cancel")]
            public async Task<IActionResult>
                CancelPayment(string code)
            {
                try
                {
                    await _paymentService
                        .CancelPaymentAsync(code);

                    return Ok(new
                    {
                        message = "Payment cancelled successfully"
                    });
                }
                catch (ApplicationException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }
        }
    
}
