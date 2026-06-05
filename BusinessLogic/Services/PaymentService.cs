using BusinessLogic.DTOs.Payment;
using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class PaymentService:  IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPayOSService _payOSService;

        public PaymentService(IUnitOfWork unitOfWork, IPayOSService payOSService)
        {
            _unitOfWork = unitOfWork;
            _payOSService = payOSService;
        }

        public async Task<CreatePaymentResponseDto> CreatePaymentAsync(Guid userId, Guid planId)
        {
            var existingActivePlan =
                await _unitOfWork
                    .PlanHistoryRepository
                    .GetLatestActiveByUserIdAsync(userId);

            if (existingActivePlan != null)
            {
                throw new ApplicationException(
                    "You already have an active plan.");
            }

            var user =
                await _unitOfWork
                    .UserRepository
                    .GetByIdAsync(userId);

            if (user == null)
            {
                throw new ApplicationException(
                    "User not found");
            }

            var plan =
                await _unitOfWork
                    .PlanRepository
                    .GetByIdAsync(planId);

            if (plan == null)
            {
                throw new ApplicationException(
                    "Plan not found");
            }

            var transactionCode =
                DateTimeOffset.UtcNow
                    .ToUnixTimeMilliseconds()
                    .ToString();

            var transaction =
                new PaymentTransaction
                {
                    TransactionId = Guid.NewGuid(),
                    UserId = userId,
                    PlanId = planId,
                    Amount = plan.Price,
                    PaymentMethod = "PayOS",
                    TransactionCode = transactionCode,
                    CreatedAt = DateTime.UtcNow
                };

            await _unitOfWork
                .PaymentTransactionRepository
                .AddAsync(transaction);

            await _unitOfWork
                .SaveAsync();

            var paymentLinkResult =
                await _payOSService
                    .CreatePaymentLinkAsync(
                        transactionCode,
                        plan.Name,
                        plan.Price);

            return new CreatePaymentResponseDto
            {
                QrCode = paymentLinkResult.QrCode,
                Bin = paymentLinkResult.Bin,
                AccountNumber = paymentLinkResult.AccountNumber,
                AccountName = paymentLinkResult.AccountName,
                Description = paymentLinkResult.Description,
                TransactionCode = transactionCode,
                Amount = plan.Price,
                PlanName = plan.Name
            };
        }

        public async Task ConfirmPaymentAsync(
    string transactionCode)
        {
            var transaction =
                await _unitOfWork
                    .PaymentTransactionRepository
                    .GetByTransactionCodeAsync(
                        transactionCode);

            if (transaction == null)
            {
                throw new ApplicationException(
                    "Transaction not found");
            }

            var existingHistory = await _unitOfWork.PlanHistoryRepository.GetAsync(
                filter: ph => ph.TransactionId == transaction.TransactionId);

            if (existingHistory.Any())
            {
                throw new ApplicationException(
                    "Payment already confirmed");
            }

            transaction.PaidAt = DateTime.UtcNow;

            var planHistory =
                new PlanHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = transaction.UserId,
                    PlanId = transaction.PlanId,
                    StartDate = DateTime.UtcNow,
                    ExpiryDate =
                        DateTime.UtcNow.AddDays(30),
                    IsActive = true,
                    TransactionId =
                        transaction.TransactionId
                };

            await _unitOfWork
                .PlanHistoryRepository
                .AddAsync(planHistory);

            await _unitOfWork
                .SaveAsync();
        }

        public async Task CancelPaymentAsync(string transactionCode)
        {
            var transaction =
                await _unitOfWork
                    .PaymentTransactionRepository
                    .GetByTransactionCodeAsync(
                        transactionCode);

            if (transaction == null)
            {
                throw new ApplicationException(
                    "Transaction not found");
            }

            var existingHistory = await _unitOfWork.PlanHistoryRepository.GetAsync(
                filter: ph => ph.TransactionId == transaction.TransactionId);

            if (existingHistory.Any())
            {
                throw new ApplicationException(
                    "Cannot cancel a successful payment.");
            }

            await _unitOfWork.SaveAsync();
        }

        public async Task<IEnumerable<PaymentTransactionDto>> GetAllTransactionsAsync()
        {
            var transactions = await _unitOfWork.PaymentTransactionRepository.GetAsync(includeProperties: "Plan");
            var planHistories = await _unitOfWork.PlanHistoryRepository.GetAllAsync();
            var successTransactionIds = planHistories.Select(ph => ph.TransactionId).ToHashSet();

            return transactions.Select(t => new PaymentTransactionDto
            {
                TransactionId = t.TransactionId,
                UserId = t.UserId,
                PlanId = t.PlanId,
                PlanName = t.Plan?.Name ?? string.Empty,
                Amount = t.Amount,
                PaymentMethod = t.PaymentMethod,
                TransactionCode = t.TransactionCode,
                CreatedAt = t.CreatedAt,
                PaidAt = t.PaidAt,
                Status = successTransactionIds.Contains(t.TransactionId)
                    ? "Success"
                    : (DateTime.UtcNow < t.CreatedAt.AddMinutes(5) ? "Pending" : "Expired")
            });
        }

        public async Task<IEnumerable<PaymentTransactionDto>> GetTransactionsByUserIdAsync(Guid userId)
        {
            var transactions = await _unitOfWork.PaymentTransactionRepository.GetAsync(
                filter: t => t.UserId == userId,
                includeProperties: "Plan");

            var planHistories = await _unitOfWork.PlanHistoryRepository.GetAsync(
                filter: ph => ph.UserId == userId);

            var successTransactionIds = planHistories.Select(ph => ph.TransactionId).ToHashSet();

            return transactions.Select(t => new PaymentTransactionDto
            {
                TransactionId = t.TransactionId,
                UserId = t.UserId,
                PlanId = t.PlanId,
                PlanName = t.Plan?.Name ?? string.Empty,
                Amount = t.Amount,
                PaymentMethod = t.PaymentMethod,
                TransactionCode = t.TransactionCode,
                CreatedAt = t.CreatedAt,
                PaidAt = t.PaidAt,
                Status = successTransactionIds.Contains(t.TransactionId)
                    ? "Success"
                    : (DateTime.UtcNow < t.CreatedAt.AddMinutes(5) ? "Pending" : "Expired")
            });
        }
    }
}
