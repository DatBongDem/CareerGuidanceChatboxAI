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

        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public User? User { get; set; }

        public decimal Price { get; set; }

        public DateTime TransactionDate { get; set; }

        public string Method { get; set; } = string.Empty;

        public string NamePlan { get; set; } = string.Empty;

        public DateTime Expiry { get; set; }
    }
}
