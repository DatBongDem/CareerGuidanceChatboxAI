using DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IRefreshTokenRepository
        : IGenericRepository<RefreshToken, Guid>
    {
        Task<RefreshToken?> GetValidTokenAsync(string refreshToken);

        Task<RefreshToken?> GetByTokenAsync(string refreshToken);
    }
}
