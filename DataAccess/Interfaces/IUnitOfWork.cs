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

        IRefreshTokenRepository RefreshTokenRepository { get; }
        IEmailVerificationRepository EmailVerificationRepository { get; }
        Task<int> SaveAsync();
    }
}
