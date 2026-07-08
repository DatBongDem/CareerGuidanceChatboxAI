namespace BusinessLogic.DTOs.Finance
{
    public class FinanceSummaryResponseDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal NetProfit { get; set; }
        public int SuccessfulTransactions { get; set; }
    }
}
