using System.Collections.Generic;

namespace BusinessLogic.DTOs.Finance
{
    public class ExpenseSummaryResponseDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalOperationalExpenses { get; set; }
        public Dictionary<string, decimal> Breakdown { get; set; } = new();
        public IEnumerable<ExpenseDto> Expenses { get; set; } = new List<ExpenseDto>();
    }
}
