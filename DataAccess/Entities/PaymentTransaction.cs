using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class PaymentTransaction
    {
        [Key]
        public Guid TransactionId { get; set; }

        public Guid UserId { get; set; }

        public User? User { get; set; }

        public Guid PlanId { get; set; }

        public Plan? Plan { get; set; }

        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;
        // QR, Banking, Momo

        public string Status { get; set; } = string.Empty;
        // Pending, Success, Failed

        public string TransactionCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? PaidAt { get; set; }
    }
}
