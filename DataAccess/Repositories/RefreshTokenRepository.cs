using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class RefreshTokenRepository
        : GenericRepository<RefreshToken, Guid>,
          IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<RefreshToken?> GetValidTokenAsync(string refreshToken)
        {
            var tokens = await _dbSet
                .Where(x =>
                    x.RevokedAt == null &&
                    x.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

            return tokens.FirstOrDefault(x =>
                BCrypt.Net.BCrypt.Verify(refreshToken, x.TokenHash));
        }

        public async Task<RefreshToken?> GetByTokenAsync(string refreshToken)
        {
            var tokens = await _dbSet.ToListAsync();

            return tokens.FirstOrDefault(x =>
                BCrypt.Net.BCrypt.Verify(refreshToken, x.TokenHash));
        }
    }
}
