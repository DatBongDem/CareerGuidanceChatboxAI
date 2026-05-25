using System;

namespace BusinessLogic.DTOs.Plan
{
    public class PlanHistoryDto
    {
        public Guid Id { get; set; }

        public decimal Price { get; set; }

        public DateTime TransactionDate { get; set; }

        public string Method { get; set; } = string.Empty;

        public string NamePlan { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime Expiry { get; set; }
    }
}
