using DataAccess.Interfaces;
using System;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository UserRepository { get; }

        IRoleRepository RoleRepository { get; }

        IPlanRepository PlanRepository { get; }

        IPlanHistoryRepository PlanHistoryRepository { get; }

        IPaymentTransactionRepository PaymentTransactionRepository { get; }

        IRefreshTokenRepository  RefreshTokenRepository { get; }

        IEmailVerificationRepository  EmailVerificationRepository { get; }

        IChatHistoryRepository ChatHistoryRepository { get; }

        IQuestionCategoryRepository QuestionCategoryRepository { get; }

        IQuestionRepository QuestionRepository { get; }

        IQuestionOptionRepository QuestionOptionRepository { get; }

        Task<int> SaveAsync();
    }
}
