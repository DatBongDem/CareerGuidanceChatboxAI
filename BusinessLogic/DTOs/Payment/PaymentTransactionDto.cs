using System;

namespace BusinessLogic.DTOs.Payment
{
    public class PaymentTransactionDto
    {
        public Guid TransactionId { get; set; }
        public Guid UserId { get; set; }
        public Guid PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string TransactionCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
