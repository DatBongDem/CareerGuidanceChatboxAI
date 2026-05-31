using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.Payment
{
    public class CreatePaymentResponseDto
    {
        public string QrUrl { get; set; } = string.Empty;

        public string TransactionCode { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string PlanName { get; set; } = string.Empty;
    }
}
