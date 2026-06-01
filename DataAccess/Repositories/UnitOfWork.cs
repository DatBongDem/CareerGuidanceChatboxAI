using DataAccess.DataContext;
using DataAccess.Interfaces;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IUserRepository UserRepository { get; private set; }

        public IRoleRepository RoleRepository { get; private set; }

        public IPlanRepository PlanRepository { get; private set; }

        public IPlanHistoryRepository PlanHistoryRepository { get; private set; }

        public IEmailVerificationRepository EmailVerificationRepository { get; private set; }

        public IRefreshTokenRepository RefreshTokenRepository { get; private set; }

      
        public IUniversityRepository Universities { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

            UserRepository = new UserRepository(_context);

            RoleRepository = new RoleRepository(_context);

            PlanRepository = new PlanRepository(_context);

            PlanHistoryRepository = new PlanHistoryRepository(_context);

            EmailVerificationRepository = new EmailVerificationRepository(_context);

            RefreshTokenRepository = new RefreshTokenRepository(_context);

         
            Universities = new UniversityRepository(_context);
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
