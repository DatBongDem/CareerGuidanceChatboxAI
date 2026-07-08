using BusinessLogic.DTOs.Finance;
using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class FinanceService : IFinanceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FinanceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<FinanceSummaryResponseDto> GetFinanceSummaryAsync(int month, int year)
        {
            // 1. Gross Revenue Pro (Individual packages)
            var proTransactions = await _unitOfWork.PaymentTransactionRepository.GetAsync(
                filter: t => t.PaidAt != null && t.PaidAt.Value.Month == month && t.PaidAt.Value.Year == year
            );
            decimal grossPro = proTransactions.Sum(t => t.Amount);

            // 2. Gross Revenue Edu (School packages)
            var eduRegistrations = await _unitOfWork.EduRegistrationRepository.GetAsync(
                filter: r => (r.Status == "Paid" || r.Status == "Completed") && r.CreatedAt.Month == month && r.CreatedAt.Year == year,
                includeProperties: "Plan"
            );
            decimal grossEdu = eduRegistrations.Sum(r => (r.Plan?.Price ?? 0) * r.StudentCount);

            decimal grossRevenue = grossPro + grossEdu;

            // 3. Successful Transactions
            int successfulTransactions = proTransactions.Count() + eduRegistrations.Count();

            // 4. Expenses
            var expenses = await _unitOfWork.OperationalExpenseRepository.GetAsync(
                filter: e => e.Date.Month == month && e.Date.Year == year
            );
            decimal totalExpense = expenses.Sum(e => e.Amount);

            // 5. Net Profit
            decimal netProfit = grossRevenue - totalExpense;

            return new FinanceSummaryResponseDto
            {
                Month = month,
                Year = year,
                GrossRevenue = grossRevenue,
                NetProfit = netProfit,
                SuccessfulTransactions = successfulTransactions
            };
        }

        public async Task<ExpenseSummaryResponseDto> GetExpensesAsync(int month, int year)
        {
            var expenses = await _unitOfWork.OperationalExpenseRepository.GetAsync(
                filter: e => e.Date.Month == month && e.Date.Year == year
            );

            decimal totalExpense = expenses.Sum(e => e.Amount);

            var categories = new[]
            {
                "AI API & Infrastructure",
                "Personnel",
                "Marketing",
                "Operational",
                "Miscellaneous"
            };

            var breakdown = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            foreach (var cat in categories)
            {
                breakdown[cat] = expenses
                    .Where(e => string.Equals(e.Category, cat, StringComparison.OrdinalIgnoreCase))
                    .Sum(e => e.Amount);
            }

            var expenseDtos = expenses.Select(e => new ExpenseDto
            {
                Id = e.Id,
                Category = e.Category,
                Description = e.Description,
                Amount = e.Amount,
                Date = e.Date,
                CreatedAt = e.CreatedAt
            });

            return new ExpenseSummaryResponseDto
            {
                Month = month,
                Year = year,
                TotalOperationalExpenses = totalExpense,
                Breakdown = breakdown,
                Expenses = expenseDtos
            };
        }

        public async Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto dto)
        {
            string category = NormalizeCategory(dto.Category);

            var expense = new OperationalExpense
            {
                Id = Guid.NewGuid(),
                Category = category,
                Description = dto.Description,
                Amount = dto.Amount,
                Date = dto.Date.ToUniversalTime(),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.OperationalExpenseRepository.AddAsync(expense);
            await _unitOfWork.SaveAsync();

            return new ExpenseDto
            {
                Id = expense.Id,
                Category = expense.Category,
                Description = expense.Description,
                Amount = expense.Amount,
                Date = expense.Date,
                CreatedAt = expense.CreatedAt
            };
        }

        public async Task<ExpenseDto> UpdateExpenseAsync(Guid id, UpdateExpenseDto dto)
        {
            var expense = await _unitOfWork.OperationalExpenseRepository.GetByIdAsync(id);
            if (expense == null)
            {
                throw new ApplicationException("Không tìm thấy khoản chi phí này.");
            }

            string category = NormalizeCategory(dto.Category);

            expense.Category = category;
            expense.Description = dto.Description;
            expense.Amount = dto.Amount;
            expense.Date = dto.Date.ToUniversalTime();

            await _unitOfWork.OperationalExpenseRepository.UpdateAsync(expense);
            await _unitOfWork.SaveAsync();

            return new ExpenseDto
            {
                Id = expense.Id,
                Category = expense.Category,
                Description = expense.Description,
                Amount = expense.Amount,
                Date = expense.Date,
                CreatedAt = expense.CreatedAt
            };
        }

        private string NormalizeCategory(string category)
        {
            var validCategories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "AI API & Infrastructure", "AI API & Infrastructure" },
                { "Personnel", "Personnel" },
                { "Marketing", "Marketing" },
                { "Operational", "Operational" },
                { "Miscellaneous", "Miscellaneous" }
            };

            if (validCategories.TryGetValue(category, out var normalized))
            {
                return normalized;
            }

            throw new ApplicationException("Danh mục chi phí không hợp lệ. Chỉ chấp nhận: AI API & Infrastructure, Personnel, Marketing, Operational, Miscellaneous");
        }
    }
}
