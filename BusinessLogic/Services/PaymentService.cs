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
                    Status = "Pending",
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

            if (transaction.Status == "Success")
            {
                throw new ApplicationException(
                    "Payment already confirmed");
            }

            transaction.Status = "Success";

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

            if (transaction.Status == "Success")
            {
                throw new ApplicationException(
                    "Cannot cancel a successful payment.");
            }

            if (transaction.Status == "Cancelled")
            {
                throw new ApplicationException(
                    "Payment has already been cancelled.");
            }

            transaction.Status = "Cancelled";

            await _unitOfWork.SaveAsync();
        }
    }
}
