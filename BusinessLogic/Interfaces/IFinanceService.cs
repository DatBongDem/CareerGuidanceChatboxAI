using BusinessLogic.DTOs.Finance;
using System;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IFinanceService
    {
        Task<FinanceSummaryResponseDto> GetFinanceSummaryAsync(int month, int year);
        Task<ExpenseSummaryResponseDto> GetExpensesAsync(int month, int year);
        Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto dto);
        Task<ExpenseDto> UpdateExpenseAsync(Guid id, UpdateExpenseDto dto);
    }
}
