using DataAccess.Entities;
using System;

namespace DataAccess.Interfaces
{
    public interface IDailyWebVisitRepository : IGenericRepository<DailyWebVisit, Guid>
    {
    }
}
