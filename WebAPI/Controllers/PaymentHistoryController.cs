using BusinessLogic.DTOs.Payment;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/payment-history")]
    public class PaymentHistoryController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentHistoryController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        [Authorize(Roles = "ACCOUNTANT")]
        public async Task<ActionResult<IEnumerable<PaymentTransactionDto>>> GetAllTransactions()
        {
            var transactions = await _paymentService.GetAllTransactionsAsync();
            return Ok(transactions);
        }

        [HttpGet("my-history")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PaymentTransactionDto>>> GetMyTransactions()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var transactions = await _paymentService.GetTransactionsByUserIdAsync(userId);
            return Ok(transactions);
        }
    }
}
