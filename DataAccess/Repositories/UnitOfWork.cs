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


      
        public IUniversityRepository Universities { get; private set; }

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

         
            Universities = new UniversityRepository(_context);

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
