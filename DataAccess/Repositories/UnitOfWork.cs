using DataAccess.DataContext;
using DataAccess.Interfaces;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IUserRepository UserRepository { get; }
        public IRoleRepository RoleRepository { get; }
        public IPlanRepository PlanRepository { get; }
        public IPlanHistoryRepository PlanHistoryRepository { get; }
        public IPaymentTransactionRepository PaymentTransactionRepository { get; }

        public IRefreshTokenRepository RefreshTokenRepository { get; }

        public IEmailVerificationRepository EmailVerificationRepository { get; }

        public IChatHistoryRepository  ChatHistoryRepository { get; }
        public IQuestionCategoryRepository QuestionCategoryRepository { get; }
        public IQuestionRepository QuestionRepository { get; }
        public IQuestionOptionRepository QuestionOptionRepository { get; }


      
        public IUniversityRepository UniversityRepository { get; private set; }
        public IUserAnswerRepository UserAnswerRepository { get; private set; }
        public IAiEvaluationRepository AiEvaluationRepository { get; private set; }
        public IUserAiSummaryRepository UserAiSummaryRepository { get; private set; }
        public IChatAiSessionRepository ChatAiSessionRepository { get; private set; }
        public IChatAiAnswerRepository ChatAiAnswerRepository { get; private set; }
        public IChatAiSummaryRepository ChatAiSummaryRepository { get; private set; }
        public IRecommendationRepository RecommendationRepository { get; private set; }
        public IUserProfileRepository   UserProfileRepository { get; private set; }
        public IMajorRepository MajorRepository { get; private set; }
        public ISkillRepository SkillRepository { get; private set; }
        public IMajorSkillRepository MajorSkillRepository { get; private set; }
        public IUniversityMajorRepository UniversityMajorRepository { get; }
        public IUniversityMajorMethodRepository UniversityMajorMethodRepository { get; }
        public IEduRegistrationRepository EduRegistrationRepository { get; }
        public IEduActivationKeyRepository EduActivationKeyRepository { get; }
        public UnitOfWork(ApplicationDbContext context)
        { }
        public UnitOfWork(
            ApplicationDbContext context,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPlanRepository planRepository,
            IPlanHistoryRepository planHistoryRepository,
            IPaymentTransactionRepository
                paymentTransactionRepository,
            IRefreshTokenRepository
                refreshTokenRepository,
            IEmailVerificationRepository
                emailVerificationRepository,
            IChatHistoryRepository chatHistoryRepository,
            IQuestionCategoryRepository questionCategoryRepository,
            IQuestionRepository questionRepository,
            IQuestionOptionRepository questionOptionRepository)

        {
            _context = context;

            UserRepository = userRepository;

            RoleRepository = roleRepository;

            PlanRepository = planRepository;

            PlanHistoryRepository =
                planHistoryRepository;

            PaymentTransactionRepository =
                paymentTransactionRepository;


            RefreshTokenRepository = new RefreshTokenRepository(_context);


            UniversityRepository = new UniversityRepository(_context);

            UserAnswerRepository = new UserAnswerRepository(_context);
            AiEvaluationRepository = new AiEvaluationRepository(_context);
            UserAiSummaryRepository = new UserAiSummaryRepository(_context);
            ChatAiSessionRepository = new ChatAiSessionRepository(_context);
            ChatAiAnswerRepository = new ChatAiAnswerRepository(_context);
            ChatAiSummaryRepository = new ChatAiSummaryRepository(_context);
            RecommendationRepository = new RecommendationRepository(_context);
            UserProfileRepository = new UserProfileRepository(_context);
            MajorRepository = new MajorRepository(_context);
            SkillRepository = new SkillRepository(_context);
            MajorSkillRepository = new MajorSkillRepository(_context);
            UniversityMajorRepository = new UniversityMajorRepository(_context);
            UniversityMajorMethodRepository = new UniversityMajorMethodRepository(_context);
            EduRegistrationRepository = new EduRegistrationRepository(_context);
            EduActivationKeyRepository = new EduActivationKeyRepository(_context);
            RefreshTokenRepository =
                refreshTokenRepository;

            EmailVerificationRepository =
                emailVerificationRepository;
            ChatHistoryRepository = chatHistoryRepository;
            QuestionCategoryRepository = questionCategoryRepository;
            QuestionRepository = questionRepository;
            QuestionOptionRepository = questionOptionRepository;

        }

        public async Task<int> SaveAsync()
        {
            return await _context
                .SaveChangesAsync(); 
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
