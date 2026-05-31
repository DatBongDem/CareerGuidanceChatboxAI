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
            IChatHistoryRepository chatHistoryRepository)
        {
            _context = context;

            UserRepository = userRepository;

            RoleRepository = roleRepository;

            PlanRepository = planRepository;

            PlanHistoryRepository =
                planHistoryRepository;

            PaymentTransactionRepository =
                paymentTransactionRepository;

            RefreshTokenRepository =
                refreshTokenRepository;

            EmailVerificationRepository =
                emailVerificationRepository;
            ChatHistoryRepository = chatHistoryRepository;
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
