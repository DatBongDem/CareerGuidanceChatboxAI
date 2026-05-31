using DataAccess.Entities;
using DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IPlanHistoryRepository
         : IGenericRepository<PlanHistory, Guid>
    {
        Task<IEnumerable<PlanHistory>> GetByUserIdAsync(Guid userId);

        Task<PlanHistory?> GetLatestByUserIdAsync(Guid userId);
    }
}
