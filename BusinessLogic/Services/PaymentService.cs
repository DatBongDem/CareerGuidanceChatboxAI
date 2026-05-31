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

        public PaymentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CreatePaymentResponseDto>  CreatePaymentAsync(Guid userId, Guid planId)
        {
            var user = await _unitOfWork
                .UserRepository
                .GetByIdAsync(userId);

            if (user == null)
            {
                throw new ApplicationException(
                    "User not found");
            }

            var plan = await _unitOfWork
                .PlanRepository
                .GetByIdAsync(planId);

            if (plan == null)
            {
                throw new ApplicationException(
                    "Plan not found");
            }

            var transactionCode =
                Guid.NewGuid().ToString();

            var transaction =
                new PaymentTransaction
                {
                    TransactionId = Guid.NewGuid(),
                    UserId = userId,
                    PlanId = planId,
                    Amount = plan.Price,
                    Status = "Pending",
                    PaymentMethod = "QR",
                    TransactionCode = transactionCode,
                    CreatedAt = DateTime.UtcNow
                };

            await _unitOfWork
                .PaymentTransactionRepository
                .AddAsync(transaction);

            await _unitOfWork
                .SaveAsync();

            string qrUrl =
                $"https://localhost:3000/payment-confirm" +
                $"?code={transactionCode}";

            return new CreatePaymentResponseDto
            {
                QrUrl = qrUrl,
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

            await _unitOfWork
                .PaymentTransactionRepository
                .UpdateAsync(transaction);

            var planHistory =
                new PlanHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = transaction.UserId,
                    PlanId = transaction.PlanId,
                    StartDate = DateTime.UtcNow,
                    ExpiryDate =
                        DateTime.UtcNow.AddMonths(
                            transaction.Plan!
                                .DurationInMonths),
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
    }
}
