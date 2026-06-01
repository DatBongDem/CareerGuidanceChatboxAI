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
        public IUniversityRepository Universities { get; }
        Task<int> SaveAsync();
    }
}
