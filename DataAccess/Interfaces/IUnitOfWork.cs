using DataAccess.Interfaces;
using DataAccess.Repositories;
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

        IRefreshTokenRepository RefreshTokenRepository { get; }
        IEmailVerificationRepository EmailVerificationRepository { get; }
        IUniversityRepository UniversityRepository { get; }


        IPaymentTransactionRepository PaymentTransactionRepository { get; }


        IChatHistoryRepository ChatHistoryRepository { get; }

        IQuestionCategoryRepository QuestionCategoryRepository { get; }

        IQuestionRepository QuestionRepository { get; }

        IQuestionOptionRepository QuestionOptionRepository { get; }

        IUserAnswerRepository UserAnswerRepository { get; }
        IAiEvaluationRepository AiEvaluationRepository { get; }
        IUserAiSummaryRepository UserAiSummaryRepository { get; }
        IChatAiSessionRepository ChatAiSessionRepository { get; }
        IChatAiAnswerRepository ChatAiAnswerRepository { get; }
        IChatAiSummaryRepository ChatAiSummaryRepository { get; }

        IRecommendationRepository RecommendationRepository { get; }

        IUserProfileRepository UserProfileRepository { get; }
        IMajorRepository MajorRepository { get; }

        ISkillRepository SkillRepository { get; }
        IMajorSkillRepository MajorSkillRepository { get; }
        IUniversityMajorRepository UniversityMajorRepository { get; }
        IUniversityMajorMethodRepository UniversityMajorMethodRepository { get; }

        Task<int> SaveAsync();
    }
}
