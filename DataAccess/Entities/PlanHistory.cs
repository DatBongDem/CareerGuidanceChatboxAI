using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class PlanHistory
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public User? User { get; set; }

        public Guid PlanId { get; set; }

        public Plan? Plan { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsActive { get; set; }

        public Guid TransactionId { get; set; }

        public PaymentTransaction? Transaction { get; set; }
    }
}
